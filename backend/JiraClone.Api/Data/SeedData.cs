using JiraClone.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Data;
public static class SeedData
{
    public static async Task InitializeAsync(JiraDbContext db)
    {
        if (await db.Projects.AnyAsync()) return;
        var users = new[] { new User { Name = "Darshan", Email = "darshan@example.com", Avatar = "DB" }, new User { Name = "Aarav", Email = "aarav@example.com", Avatar = "AR" }, new User { Name = "Priya", Email = "priya@example.com", Avatar = "PS" } };
        db.Users.AddRange(users);
        var project = new Project { Key = "ACME", Name = "Acme Platform", Description = "A production-style Jira clone workspace." };
        var sprint = new Sprint { Name = "Sprint 24", StartDate = DateTime.UtcNow.Date.AddDays(-3), EndDate = DateTime.UtcNow.Date.AddDays(11) };
        db.Projects.Add(project); db.Sprints.Add(sprint); await db.SaveChangesAsync();
        db.Issues.AddRange(
            new Issue { ProjectId = project.Id, Number = 101, Title = "Design the new dashboard", Description = "Create responsive dashboard UX and information architecture.", Status = IssueStatus.InProgress, Priority = IssuePriority.High, Type = IssueType.Story, AssigneeId = users[0].Id, SprintId = sprint.Id },
            new Issue { ProjectId = project.Id, Number = 102, Title = "Add SQLite persistence", Description = "Persist projects, sprints and issues using EF Core SQLite.", Status = IssueStatus.Done, Priority = IssuePriority.Medium, Type = IssueType.Task, AssigneeId = users[1].Id, SprintId = sprint.Id },
            new Issue { ProjectId = project.Id, Number = 103, Title = "Fix mobile board overflow", Description = "Board columns should remain usable on narrow screens.", Status = IssueStatus.Todo, Priority = IssuePriority.High, Type = IssueType.Bug, AssigneeId = users[2].Id, SprintId = sprint.Id },
            new Issue { ProjectId = project.Id, Number = 104, Title = "Create issue filters", Description = "Support quick filtering by type, priority and assignee.", Status = IssueStatus.Backlog, Priority = IssuePriority.Low, Type = IssueType.Story, AssigneeId = users[0].Id },
            new Issue { ProjectId = project.Id, Number = 105, Title = "API documentation", Description = "Document the REST endpoints in Swagger.", Status = IssueStatus.InReview, Priority = IssuePriority.Medium, Type = IssueType.Task, AssigneeId = users[1].Id, SprintId = sprint.Id }
        );
        await db.SaveChangesAsync();
    }
}
