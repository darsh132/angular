using JiraClone.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Data;

public static class SeedData
{
    public static async Task InitializeAsync(JiraDbContext db)
    {
        if (await db.Projects.AnyAsync()) return;

        var users = new[]
        {
            new User { Name = "Darshan", Email = "darshan@example.com", Avatar = "DB", PasswordHash = "demo123" },
            new User { Name = "Aarav", Email = "aarav@example.com", Avatar = "AR", PasswordHash = "demo123" },
            new User { Name = "Priya", Email = "priya@example.com", Avatar = "PS", PasswordHash = "demo123" }
        };
        db.Users.AddRange(users);

        var project = new Project { Key = "ACME", Name = "Acme Platform", Description = "A production-style Jira clone workspace." };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var sprint = new Sprint
        {
            ProjectId = project.Id,
            Name = "Sprint 24",
            Goal = "Deliver the first production-style project management vertical slice.",
            Status = SprintStatus.Active,
            StartDate = DateTime.UtcNow.Date.AddDays(-3),
            EndDate = DateTime.UtcNow.Date.AddDays(11)
        };
        db.Sprints.Add(sprint);
        await db.SaveChangesAsync();

        var now = DateTime.UtcNow;
        db.Issues.AddRange(
            new Issue { ProjectId = project.Id, Number = 101, Title = "Design the new dashboard", Description = "Create responsive dashboard UX and information architecture.", Status = IssueStatus.InProgress, Priority = IssuePriority.High, Type = IssueType.Story, AssigneeId = users[0].Id, SprintId = sprint.Id, CreatedAt = now, UpdatedAt = now },
            new Issue { ProjectId = project.Id, Number = 102, Title = "Add SQLite persistence", Description = "Persist projects, sprints and issues using EF Core SQLite.", Status = IssueStatus.Done, Priority = IssuePriority.Medium, Type = IssueType.Task, AssigneeId = users[1].Id, SprintId = sprint.Id, CreatedAt = now, UpdatedAt = now },
            new Issue { ProjectId = project.Id, Number = 103, Title = "Fix mobile board overflow", Description = "Board columns should remain usable on narrow screens.", Status = IssueStatus.Todo, Priority = IssuePriority.High, Type = IssueType.Bug, AssigneeId = users[2].Id, SprintId = sprint.Id, CreatedAt = now, UpdatedAt = now },
            new Issue { ProjectId = project.Id, Number = 104, Title = "Create issue filters", Description = "Support quick filtering by type, priority and assignee.", Status = IssueStatus.Backlog, Priority = IssuePriority.Low, Type = IssueType.Story, AssigneeId = users[0].Id, CreatedAt = now, UpdatedAt = now },
            new Issue { ProjectId = project.Id, Number = 105, Title = "API documentation", Description = "Document the REST endpoints in Swagger.", Status = IssueStatus.InReview, Priority = IssuePriority.Medium, Type = IssueType.Task, AssigneeId = users[1].Id, SprintId = sprint.Id, CreatedAt = now, UpdatedAt = now }
        );
        await db.SaveChangesAsync();
    }
}
