using JiraClone.Api.Data;
using JiraClone.Api.Models;
using JiraClone.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Tests;

public sealed class SprintAnalyticsServiceTests
{
    private static async Task<(SqliteConnection Connection, JiraDbContext Db)> CreateDb()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<JiraDbContext>().UseSqlite(connection).Options;
        var db = new JiraDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return (connection, db);
    }

    [Fact]
    public async Task Returns_zero_metrics_for_empty_sprint()
    {
        var (connection, db) = await CreateDb();
        await using var _ = connection;
        await using var __ = db;
        var project = new Project { Key = "TEST", Name = "Test" };
        db.Projects.Add(project); await db.SaveChangesAsync();
        var sprint = new Sprint { ProjectId = project.Id, Name = "Empty", Status = SprintStatus.Active, StartDate = DateTime.UtcNow.Date, EndDate = DateTime.UtcNow.Date.AddDays(6) };
        db.Sprints.Add(sprint); await db.SaveChangesAsync();

        var result = await new SprintAnalyticsService(db).GetAsync(project.Id, sprint.Id, CancellationToken.None);

        Assert.NotNull(result); Assert.Equal(0, result!.CommittedPoints); Assert.Equal(0, result.CompletedPoints); Assert.Equal(0, result.RemainingPoints); Assert.Equal(0, result.IssueCount); Assert.Empty(result.Actual);
    }

    [Fact]
    public async Task Calculates_completed_and_remaining_points()
    {
        var (connection, db) = await CreateDb();
        await using var _ = connection;
        await using var __ = db;
        var project = new Project { Key = "TEST", Name = "Test" };
        db.Projects.Add(project); await db.SaveChangesAsync();
        var sprint = new Sprint { ProjectId = project.Id, Name = "Sprint 1", Status = SprintStatus.Completed, StartDate = DateTime.UtcNow.Date.AddDays(-6), EndDate = DateTime.UtcNow.Date };
        db.Sprints.Add(sprint); await db.SaveChangesAsync();
        db.Issues.AddRange(
            new Issue { ProjectId = project.Id, SprintId = sprint.Id, Key = "TEST-1", Title = "Done", Description = "", Status = IssueStatus.Done, Priority = IssuePriority.Medium, Type = IssueType.Task, StoryPoints = 8 },
            new Issue { ProjectId = project.Id, SprintId = sprint.Id, Key = "TEST-2", Title = "Todo", Description = "", Status = IssueStatus.Todo, Priority = IssuePriority.Medium, Type = IssueType.Task, StoryPoints = 5 });
        await db.SaveChangesAsync();

        var result = await new SprintAnalyticsService(db).GetAsync(project.Id, sprint.Id, CancellationToken.None);

        Assert.NotNull(result); Assert.Equal(13, result!.CommittedPoints); Assert.Equal(8, result.CompletedPoints); Assert.Equal(5, result.RemainingPoints); Assert.Equal(2, result.IssueCount); Assert.Equal(1, result.CompletedIssueCount); Assert.NotEmpty(result.Ideal);
    }

    [Fact]
    public async Task Returns_null_for_wrong_project()
    {
        var (connection, db) = await CreateDb();
        await using var _ = connection;
        await using var __ = db;
        var p1 = new Project { Key = "ONE", Name = "One" }; var p2 = new Project { Key = "TWO", Name = "Two" };
        db.Projects.AddRange(p1, p2); await db.SaveChangesAsync();
        var sprint = new Sprint { ProjectId = p1.Id, Name = "Sprint", Status = SprintStatus.Planned, StartDate = DateTime.UtcNow.Date, EndDate = DateTime.UtcNow.Date.AddDays(1) };
        db.Sprints.Add(sprint); await db.SaveChangesAsync();

        var result = await new SprintAnalyticsService(db).GetAsync(p2.Id, sprint.Id, CancellationToken.None);

        Assert.Null(result);
    }
}
