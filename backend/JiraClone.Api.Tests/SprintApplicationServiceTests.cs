using JiraClone.Api.Data;
using JiraClone.Api.Models;
using JiraClone.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Tests;

public sealed class SprintApplicationServiceTests : IAsyncLifetime
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
        _db.Projects.Add(new Project { Key = "TEST", Name = "Test Project", Description = "Sprint tests" });
        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task Create_sets_project_goal_and_planned_status()
    {
        var project = await _db.Projects.SingleAsync();
        var service = new SprintApplicationService(_db);

        var sprint = await service.CreateAsync(
            project.Id,
            "Sprint 1",
            "Ship the first slice",
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddDays(14),
            CancellationToken.None);

        Assert.Equal(project.Id, sprint.ProjectId);
        Assert.Equal("Ship the first slice", sprint.Goal);
        Assert.Equal(SprintStatus.Planned, sprint.Status);
    }

    [Fact]
    public async Task Start_rejects_second_active_sprint_for_same_project()
    {
        var project = await _db.Projects.SingleAsync();
        var service = new SprintApplicationService(_db);
        var first = await service.CreateAsync(project.Id, "Sprint 1", null, DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(14), CancellationToken.None);
        var second = await service.CreateAsync(project.Id, "Sprint 2", null, DateTime.UtcNow.Date.AddDays(14), DateTime.UtcNow.Date.AddDays(28), CancellationToken.None);

        await service.StartAsync(first.Id, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.StartAsync(second.Id, CancellationToken.None));
    }

    [Fact]
    public async Task Assign_issue_rejects_issue_from_another_project()
    {
        var firstProject = await _db.Projects.SingleAsync();
        var secondProject = new Project { Key = "OTHER", Name = "Other Project" };
        _db.Projects.Add(secondProject);
        await _db.SaveChangesAsync();

        var sprintService = new SprintApplicationService(_db);
        var sprint = await sprintService.CreateAsync(firstProject.Id, "Sprint 1", null, DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(14), CancellationToken.None);
        var issue = new Issue
        {
            ProjectId = secondProject.Id,
            Number = 1,
            Title = "Other issue",
            Status = IssueStatus.Todo,
            Priority = IssuePriority.Medium,
            Type = IssueType.Task,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => sprintService.AssignIssueAsync(issue.Id, sprint.Id, CancellationToken.None));
    }
}
