using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JiraClone.Api.Data;
using JiraClone.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace JiraClone.Api.Services;

public sealed class AuthService(JiraDbContext db, IConfiguration configuration)
{
    private readonly PasswordHasher<User> hasher = new();

    public async Task<AuthResult?> AuthenticateAsync(string email, string password, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email.Trim(), ct);
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Failed) return null;
        return await CreateAuthResultAsync(user, ct);
    }

    public async Task<AuthResult?> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        var hash = HashToken(refreshToken);
        var stored = await db.RefreshTokens.Include(x => x.User).SingleOrDefaultAsync(x => x.TokenHash == hash, ct);
        if (stored is null || stored.ExpiresAt <= DateTime.UtcNow) return null;

        // Atomically claim the token so concurrent refresh requests cannot both rotate it.
        var now = DateTime.UtcNow;
        var claimed = await db.RefreshTokens
            .Where(x => x.TokenHash == hash && x.RevokedAt == null && x.ExpiresAt > now)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.RevokedAt, now), ct);

        if (claimed == 0)
        {
            // A previously rotated token was presented again: treat it as token-family compromise.
            await RevokeTokenFamilyAsync(stored.UserId, now, ct);
            return null;
        }

        var result = await CreateAuthResultAsync(stored.User, ct, persist: false);
        stored.ReplacedByTokenHash = HashToken(result.RefreshToken);
        await db.SaveChangesAsync(ct);
        return result;
    }

    public async Task<bool> RevokeAsync(string refreshToken, CancellationToken ct)
    {
        var stored = await db.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == HashToken(refreshToken), ct);
        if (stored is null || stored.RevokedAt is not null) return false;
        stored.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    private async Task RevokeTokenFamilyAsync(int userId, DateTime revokedAt, CancellationToken ct)
    {
        await db.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.RevokedAt, revokedAt), ct);
    }

    private async Task<AuthResult> CreateAuthResultAsync(User user, CancellationToken ct, bool persist = true)
    {
        var refreshToken = CreateRefreshToken();
        db.RefreshTokens.Add(new RefreshToken { UserId = user.Id, TokenHash = HashToken(refreshToken), CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshDays()) });
        if (persist) await db.SaveChangesAsync(ct);
        return new AuthResult(CreateAccessToken(user), refreshToken, new AuthUser(user.Id, user.Name, user.Email, user.Avatar, user.Role));
    }

    private string CreateAccessToken(User user)
    {
        var key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT signing key is not configured.");
        if (key.Length < 32) throw new InvalidOperationException("JWT signing key must contain at least 32 characters.");
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.Name), new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Role, user.Role) };
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddMinutes(GetAccessMinutes()), signingCredentials: credentials));
    }

    private int GetAccessMinutes() => Math.Clamp(configuration.GetValue("Jwt:AccessTokenMinutes", 15), 5, 60);
    private int GetRefreshDays() => Math.Clamp(configuration.GetValue("Jwt:RefreshTokenDays", 30), 1, 90);
    private static string CreateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
}

public sealed record AuthResult(string Token, string RefreshToken, AuthUser User);
public sealed record AuthUser(int Id, string Name, string Email, string Avatar, string Role);
