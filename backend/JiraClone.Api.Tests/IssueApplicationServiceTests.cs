using JiraClone.Api.Data;
using JiraClone.Api.Models;
using JiraClone.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Tests;

public sealed class IssueApplicationServiceTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private JiraDbContext _db = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<JiraDbContext>().UseSqlite(_connection).Options;
        _db = new JiraDbContext(options);
        await _db.Database.EnsureCreatedAsync();
        _db.Users.Add(new User { Name = "Test User", Email = "test@example.com", Avatar = "TU", PasswordHash = "test" });
        _db.Projects.Add(new Project { Key = "TEST", Name = "Test Project", Description = "Integration test project" });
        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task Create_creates_issue_and_activity()
    {
        var project = await _db.Projects.SingleAsync();
        var service = new IssueApplicationService(_db);

        var issue = await service.CreateAsync(
            new CreateIssueCommand(project.Id, "Build API", "Create the issue endpoint", IssueStatus.Todo, IssuePriority.High, IssueType.Task, StoryPoints: 5),
            CancellationToken.None);

        Assert.Equal(1, issue.Number);
        Assert.Equal("Build API", issue.Title);
        Assert.Equal(5, issue.StoryPoints);
        var activity = await _db.IssueActivities.SingleAsync();
        Assert.Equal(IssueActivityType.Created, activity.Type);
        Assert.Equal("Todo", activity.NewValue);
    }

    [Fact]
    public async Task Move_creates_status_activity()
    {
        var project = await _db.Projects.SingleAsync();
        var service = new IssueApplicationService(_db);
        var issue = await service.CreateAsync(
            new CreateIssueCommand(project.Id, "Review API", null, IssueStatus.Todo, IssuePriority.Medium, IssueType.Task),
            CancellationToken.None);

        await service.MoveAsync(issue.Id, IssueStatus.InProgress, CancellationToken.None);

        var saved = await _db.Issues.SingleAsync(x => x.Id == issue.Id);
        var activity = await _db.IssueActivities.OrderByDescending(x => x.Id).FirstAsync();
        Assert.Equal(IssueStatus.InProgress, saved.Status);
        Assert.Equal(IssueActivityType.StatusChanged, activity.Type);
        Assert.Equal("Todo", activity.OldValue);
        Assert.Equal("InProgress", activity.NewValue);
    }

    [Fact]
    public async Task Invalid_transition_does_not_change_issue()
    {
        var project = await _db.Projects.SingleAsync();
        var service = new IssueApplicationService(_db);
        var issue = await service.CreateAsync(
            new CreateIssueCommand(project.Id, "Invalid flow", null, IssueStatus.Backlog, IssuePriority.Low, IssueType.Task),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.MoveAsync(issue.Id, IssueStatus.Done, CancellationToken.None));

        var saved = await _db.Issues.SingleAsync(x => x.Id == issue.Id);
        Assert.Equal(IssueStatus.Backlog, saved.Status);
    }
}
