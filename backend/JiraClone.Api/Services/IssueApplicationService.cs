using JiraClone.Api.Data;
using JiraClone.Api.Domain;
using JiraClone.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Services;

public sealed class IssueApplicationService(JiraDbContext db)
{
    public async Task<Issue> CreateAsync(CreateIssueCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Title)) throw new ArgumentException("Issue title is required.", nameof(command));
        if (command.StoryPoints < 0) throw new ArgumentOutOfRangeException(nameof(command.StoryPoints));
        var project = await db.Projects.FirstOrDefaultAsync(x => x.Id == command.ProjectId, ct) ?? throw new KeyNotFoundException("Project not found.");
        if (command.AssigneeId is not null && !await db.Users.AnyAsync(x => x.Id == command.AssigneeId, ct)) throw new KeyNotFoundException("Assignee not found.");
        if (command.SprintId is not null)
        {
            var sprint = await db.Sprints.FirstOrDefaultAsync(x => x.Id == command.SprintId, ct) ?? throw new KeyNotFoundException("Sprint not found.");
            if (sprint.ProjectId != project.Id) throw new InvalidOperationException("Issue and sprint must belong to the same project.");
            if (sprint.Status == SprintStatus.Completed) throw new InvalidOperationException("A completed sprint cannot receive issues.");
        }
        var number = (await db.Issues.Where(x => x.ProjectId == project.Id).MaxAsync(x => (int?)x.Number, ct) ?? 0) + 1;
        var now = DateTime.UtcNow;
        var issue = new Issue { ProjectId = project.Id, Number = number, Title = command.Title.Trim(), Description = command.Description?.Trim() ?? string.Empty, Status = command.Status, Priority = command.Priority, Type = command.Type, StoryPoints = command.StoryPoints, AssigneeId = command.AssigneeId, SprintId = command.SprintId, CreatedAt = now, UpdatedAt = now };
        db.Issues.Add(issue);
        var actor = await db.Users.OrderBy(x => x.Id).FirstOrDefaultAsync(ct);
        if (actor is not null) db.IssueActivities.Add(new IssueActivity { Issue = issue, ActorId = actor.Id, Type = IssueActivityType.Created, NewValue = issue.Status.ToString(), CreatedAt = now });
        await db.SaveChangesAsync(ct);
        return issue;
    }

    public async Task MoveAsync(int id, IssueStatus target, CancellationToken ct)
    {
        var issue = await db.Issues.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new KeyNotFoundException("Issue not found.");
        IssueWorkflow.EnsureCanTransition(issue.Status, target);
        var old = issue.Status;
        issue.Status = target;
        issue.UpdatedAt = DateTime.UtcNow;
        var actor = await db.Users.OrderBy(x => x.Id).FirstOrDefaultAsync(ct);
        if (actor is not null) db.IssueActivities.Add(new IssueActivity { IssueId = issue.Id, ActorId = actor.Id, Type = IssueActivityType.StatusChanged, OldValue = old.ToString(), NewValue = target.ToString(), CreatedAt = issue.UpdatedAt });
        await db.SaveChangesAsync(ct);
    }
}

public sealed record CreateIssueCommand(int ProjectId, string Title, string? Description, IssueStatus Status, IssuePriority Priority, IssueType Type, int? AssigneeId = null, int? SprintId = null, int StoryPoints = 0);
