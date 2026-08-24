using JiraClone.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Tests;

public sealed class MigrationTests
{
    [Fact]
    public async Task Fresh_database_applies_migrations_and_creates_required_schema()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<JiraDbContext>().UseSqlite(connection).Options;

        await using (var db = new JiraDbContext(options))
        {
            await db.Database.MigrateAsync();
            Assert.True(await db.Database.CanConnectAsync());
            Assert.NotEmpty(await db.Database.GetAppliedMigrationsAsync());
            Assert.NotNull(await db.Model.FindEntityType(typeof(IssueNumberSequence)));
        }

        await using var verification = new JiraDbContext(options);
        Assert.Empty(await verification.Projects.ToListAsync());
        Assert.Empty(await verification.Issues.ToListAsync());
    }
}
