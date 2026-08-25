using JiraClone.Api.Data;
using JiraClone.Api.Models;
using JiraClone.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JiraClone.Api.Tests;

public sealed class AuthRefreshTokenTests
{
    [Fact]
    public async Task Refresh_rotates_token_and_links_replacement()
    {
        await using var fixture = await Fixture.CreateAsync();
        var first = await fixture.Service.AuthenticateAsync(fixture.User.Email, "password", CancellationToken.None);

        Assert.NotNull(first);
        var second = await fixture.Service.RefreshAsync(first!.RefreshToken, CancellationToken.None);

        Assert.NotNull(second);
        Assert.NotEqual(first.RefreshToken, second!.RefreshToken);

        var oldHash = fixture.Hash(first.RefreshToken);
        var replacementHash = fixture.Hash(second.RefreshToken);
        var old = await fixture.Db.RefreshTokens.SingleAsync(x => x.TokenHash == oldHash);

        Assert.NotNull(old.RevokedAt);
        Assert.Equal(replacementHash, old.ReplacedByTokenHash);
        Assert.True(await fixture.Db.RefreshTokens.AnyAsync(x => x.TokenHash == replacementHash && x.RevokedAt == null));
    }

    [Fact]
    public async Task Reuse_of_rotated_token_revokes_the_remaining_token_family()
    {
        await using var fixture = await Fixture.CreateAsync();
        var first = await fixture.Service.AuthenticateAsync(fixture.User.Email, "password", CancellationToken.None);
        Assert.NotNull(first);

        var second = await fixture.Service.RefreshAsync(first!.RefreshToken, CancellationToken.None);
        Assert.NotNull(second);

        var reused = await fixture.Service.RefreshAsync(first.RefreshToken, CancellationToken.None);

        Assert.Null(reused);
        var activeTokens = await fixture.Db.RefreshTokens.CountAsync(x => x.UserId == fixture.User.Id && x.RevokedAt == null);
        Assert.Equal(0, activeTokens);
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private Fixture(SqliteConnection connection, JiraDbContext db, AuthService service, User user)
        {
            Connection = connection;
            Db = db;
            Service = service;
            User = user;
        }

        public SqliteConnection Connection { get; }
        public JiraDbContext Db { get; }
        public AuthService Service { get; }
        public User User { get; }

        public static async Task<Fixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<JiraDbContext>().UseSqlite(connection).Options;
            var db = new JiraDbContext(options);
            await db.Database.EnsureCreatedAsync();

            var user = new User { Name = "Test User", Email = "auth@test.local", Role = "User" };
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, "password");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "test-key-that-is-at-least-32-characters-long",
                    ["Jwt:AccessTokenMinutes"] = "15",
                    ["Jwt:RefreshTokenDays"] = "30"
                })
                .Build();

            return new Fixture(connection, db, new AuthService(db, configuration), user);
        }

        public string Hash(string token)
            => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await Connection.DisposeAsync();
        }
    }
}
