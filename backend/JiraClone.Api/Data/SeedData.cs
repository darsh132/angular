using JiraClone.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Data;

public static class SeedData
{
    public static async Task InitializeAsync(JiraDbContext db)
    {
        var hasher = new PasswordHasher<User>();
        var existingUsers = await db.Users.OrderBy(x => x.Id).ToListAsync();
        if (existingUsers.Count > 0)
        {
            var changed = false;
            foreach (var user in existingUsers)
            {
                if (user.PasswordHash == "demo123") { user.PasswordHash = hasher.HashPassword(user, "demo123"); changed = true; }
                if (string.IsNullOrWhiteSpace(user.Role)) { user.Role = user.Id == existingUsers[0].Id ? "Admin" : "User"; changed = true; }
            }
            if (changed) await db.SaveChangesAsync();
        }
        if (await db.Projects.AnyAsync())
        {
            await EnsureMembershipsAsync(db);
            return;
        }

        var users = new[]
        {
            new User { Name = "Darshan", Email = "darshan@example.com", Avatar = "DB", Role = "Admin" },
            new User { Name = "Aarav", Email = "aarav@example.com", Avatar = "AR", Role = "User" },
            new User { Name = "Priya", Email = "priya@example.com", Avatar = "PS", Role = "User" }
        };
        foreach (var user in users) user.PasswordHash = hasher.HashPassword(user, "demo123");
        db.Users.AddRange(users);

        var project = new Project { Key = "ACME", Name = "Acme Platform", Description = "A production-style Jira clone workspace." };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        db.ProjectMembers.AddRange(
            new ProjectMember { ProjectId = project.Id, UserId = users[1].Id, Role = ProjectRole.Member },
            new ProjectMember { ProjectId = project.Id, UserId = users[2].Id, Role = ProjectRole.Viewer });
        await db.SaveChangesAsync();

        var sprint = new Sprint { ProjectId = project.Id, Name = "Sprint 24", Goal = "Deliver the first production-style project management vertical slice.", Status = SprintStatus.Active, StartDate = DateTime.UtcNow.Date.AddDays(-3), EndDate = DateTime.UtcNow.Date.AddDays(11) };
        db.Sprints.Add(sprint);
        await db.SaveChangesAsync();

        var now = DateTime.UtcNow;
        db.Issues.AddRange(
            new Issue { ProjectId = project.Id, Number = 101, Title = "Design the new dashboard", Description = "Create responsive dashboard UX and information architecture.", Status = IssueStatus.InProgress, Priority = IssuePriority.High, Type = IssueType.Story, StoryPoints = 5, AssigneeId = users[0].Id, SprintId = sprint.Id, CreatedAt = now, UpdatedAt = now },
            new Issue { ProjectId = project.Id, Number = 102, Title = "Add SQLite persistence", Description = "Persist projects, sprints and issues using EF Core SQLite.", Status = IssueStatus.Done, Priority = IssuePriority.Medium, Type = IssueType.Task, StoryPoints = 3, AssigneeId = users[1].Id, SprintId = sprint.Id, CreatedAt = now, UpdatedAt = now },
            new Issue { ProjectId = project.Id, Number = 103, Title = "Fix mobile board overflow", Description = "Board columns should remain usable on narrow screens.", Status = IssueStatus.Todo, Priority = IssuePriority.High, Type = IssueType.Bug, StoryPoints = 3, AssigneeId = users[2].Id, SprintId = sprint.Id, CreatedAt = now, UpdatedAt = now },
            new Issue { ProjectId = project.Id, Number = 104, Title = "Create issue filters", Description = "Support quick filtering by type, priority and assignee.", Status = IssueStatus.Backlog, Priority = IssuePriority.Low, Type = IssueType.Story, StoryPoints = 5, AssigneeId = users[0].Id, CreatedAt = now, UpdatedAt = now },
            new Issue { ProjectId = project.Id, Number = 105, Title = "API documentation", Description = "Document the REST endpoints in Swagger.", Status = IssueStatus.InReview, Priority = IssuePriority.Medium, Type = IssueType.Task, StoryPoints = 2, AssigneeId = users[1].Id, SprintId = sprint.Id, CreatedAt = now, UpdatedAt = now });
        await db.SaveChangesAsync();
    }

    private static async Task EnsureMembershipsAsync(JiraDbContext db)
    {
        var projectIds = await db.Projects.Select(x => x.Id).ToListAsync();
        var users = await db.Users.OrderBy(x => x.Id).ToListAsync();
        if (users.Count < 3) return;
        foreach (var projectId in projectIds)
        {
            if (!await db.ProjectMembers.AnyAsync(x => x.ProjectId == projectId && x.UserId == users[1].Id)) db.ProjectMembers.Add(new ProjectMember { ProjectId = projectId, UserId = users[1].Id, Role = ProjectRole.Member });
            if (!await db.ProjectMembers.AnyAsync(x => x.ProjectId == projectId && x.UserId == users[2].Id)) db.ProjectMembers.Add(new ProjectMember { ProjectId = projectId, UserId = users[2].Id, Role = ProjectRole.Viewer });
        }
        await db.SaveChangesAsync();
    }
}
