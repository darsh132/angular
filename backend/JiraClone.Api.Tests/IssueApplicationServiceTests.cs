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
        _db.Projects.AddRange(
            new Project { Key = "TEST", Name = "Test Project", Description = "Integration test project" },
            new Project { Key = "OTHER", Name = "Other Project", Description = "Second project" });
        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private async Task<Project> ProjectAsync(string key = "TEST") => await _db.Projects.SingleAsync(x => x.Key == key);

    [Fact]
    public async Task Create_creates_issue_and_activity()
    {
        var project = await ProjectAsync();
        var service = new IssueApplicationService(_db);
        var issue = await service.CreateAsync(new CreateIssueCommand(project.Id, "Build API", "Create the issue endpoint", IssueStatus.Todo, IssuePriority.High, IssueType.Task, StoryPoints: 5), CancellationToken.None);
        Assert.Equal(1, issue.Number);
        Assert.Equal("Build API", issue.Title);
        Assert.Equal(5, issue.StoryPoints);
        var activity = await _db.IssueActivities.SingleAsync();
        Assert.Equal(IssueActivityType.Created, activity.Type);
        Assert.Equal("Todo", activity.NewValue);
    }

    [Fact]
    public async Task Create_allocates_sequential_project_scoped_numbers()
    {
        var project = await ProjectAsync();
        var service = new IssueApplicationService(_db);
        var first = await service.CreateAsync(new CreateIssueCommand(project.Id, "First", null, IssueStatus.Todo, IssuePriority.Low, IssueType.Task), CancellationToken.None);
        var second = await service.CreateAsync(new CreateIssueCommand(project.Id, "Second", null, IssueStatus.Todo, IssuePriority.Low, IssueType.Task), CancellationToken.None);
        Assert.Equal(1, first.Number);
        Assert.Equal(2, second.Number);
    }

    [Fact]
    public async Task Create_uses_independent_numbers_per_project()
    {
        var firstProject = await ProjectAsync("TEST");
        var secondProject = await ProjectAsync("OTHER");
        var service = new IssueApplicationService(_db);
        var first = await service.CreateAsync(new CreateIssueCommand(firstProject.Id, "First", null, IssueStatus.Todo, IssuePriority.Low, IssueType.Task), CancellationToken.None);
        var other = await service.CreateAsync(new CreateIssueCommand(secondProject.Id, "Other", null, IssueStatus.Todo, IssuePriority.Low, IssueType.Task), CancellationToken.None);
        Assert.Equal(1, first.Number);
        Assert.Equal(1, other.Number);
    }

    [Fact]
    public async Task Create_continues_after_existing_issue_number()
    {
        var project = await ProjectAsync();
        _db.Issues.Add(new Issue { ProjectId = project.Id, Number = 7, Title = "Existing", Status = IssueStatus.Done, Priority = IssuePriority.Low, Type = IssueType.Task, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync();
        var service = new IssueApplicationService(_db);
        var issue = await service.CreateAsync(new CreateIssueCommand(project.Id, "Next", null, IssueStatus.Todo, IssuePriority.Low, IssueType.Task), CancellationToken.None);
        Assert.Equal(8, issue.Number);
    }

    [Fact]
    public async Task Move_creates_status_activity()
    {
        var project = await ProjectAsync();
        var service = new IssueApplicationService(_db);
        var issue = await service.CreateAsync(new CreateIssueCommand(project.Id, "Review API", null, IssueStatus.Todo, IssuePriority.Medium, IssueType.Task), CancellationToken.None);
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
        var project = await ProjectAsync();
        var service = new IssueApplicationService(_db);
        var issue = await service.CreateAsync(new CreateIssueCommand(project.Id, "Invalid flow", null, IssueStatus.Backlog, IssuePriority.Low, IssueType.Task), CancellationToken.None);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.MoveAsync(issue.Id, IssueStatus.Done, CancellationToken.None));
        var saved = await _db.Issues.SingleAsync(x => x.Id == issue.Id);
        Assert.Equal(IssueStatus.Backlog, saved.Status);
    }
}
