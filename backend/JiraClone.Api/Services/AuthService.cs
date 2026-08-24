using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        return new AuthResult(CreateToken(user), new AuthUser(user.Id, user.Name, user.Email, user.Avatar, user.Role));
    }

    private string CreateToken(User user)
    {
        var key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT signing key is not configured.");
        if (key.Length < 32) throw new InvalidOperationException("JWT signing key must contain at least 32 characters.");
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: credentials));
    }
}

public sealed record AuthResult(string Token, AuthUser User);
public sealed record AuthUser(int Id, string Name, string Email, string Avatar, string Role);
