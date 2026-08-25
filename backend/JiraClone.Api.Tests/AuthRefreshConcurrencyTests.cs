using System.Security.Cryptography;
using System.Text;
using JiraClone.Api.Data;
using JiraClone.Api.Models;
using JiraClone.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace JiraClone.Api.Tests;

public sealed class AuthRefreshConcurrencyTests
{
    [Fact]
    public async Task Refresh_rotation_keeps_replacement_inside_the_transaction()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<JiraDbContext>().UseSqlite(connection).Options;
        await using var db = new JiraDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var user = new User { Name = "Test", Email = "auth@test.local", PasswordHash = "hash" };
        db.Users.Add(user); await db.SaveChangesAsync();
        const string raw = "refresh-token-for-test";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();
        var token = new RefreshToken { UserId = user.Id, TokenHash = hash, CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddDays(30) };
        db.RefreshTokens.Add(token); await db.SaveChangesAsync();

        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> { ["Jwt:Key"] = new string('x', 64) }).Build();
        var service = new AuthService(db, config);
        var result = await service.RefreshAsync(raw, CancellationToken.None);

        Assert.NotNull(result);
        var old = await db.RefreshTokens.SingleAsync(x => x.Id == token.Id);
        var replacementHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(result!.RefreshToken))).ToLowerInvariant();
        var replacement = await db.RefreshTokens.SingleAsync(x => x.TokenHash == replacementHash);
        Assert.NotNull(old.RevokedAt);
        Assert.Equal(replacementHash, old.ReplacedByTokenHash);
        Assert.Null(replacement.RevokedAt);
    }
}
