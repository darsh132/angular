using JiraClone.Api.Data;
using JiraClone.Api.Domain;
using JiraClone.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Services;

public sealed class IssueApplicationService(JiraDbContext db)
{
    public async Task<Issue> CreateAsync(CreateIssueCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            throw new ArgumentException("Issue title is required.", nameof(command));
        if (command.StoryPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(command.StoryPoints));

        var project = await db.Projects.FirstOrDefaultAsync(x => x.Id == command.ProjectId, ct)
            ?? throw new KeyNotFoundException("Project not found.");

        var number = (await db.Issues.Where(x => x.ProjectId == project.Id)
            .MaxAsync(x => (int?)x.Number, ct) ?? 0) + 1;
        var now = DateTime.UtcNow;
        var issue = new Issue
        {
            ProjectId = project.Id,
            Number = number,
            Title = command.Title.Trim(),
            Description = command.Description?.Trim() ?? string.Empty,
            Status = command.Status,
            Priority = command.Priority,
            Type = command.Type,
            AssigneeId = command.AssigneeId,
            SprintId = command.SprintId,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Issues.Add(issue);
        await db.SaveChangesAsync(ct);
        return issue;
    }

    public async Task MoveAsync(int id, IssueStatus target, CancellationToken ct)
    {
        var issue = await db.Issues.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("Issue not found.");
        IssueWorkflow.EnsureCanTransition(issue.Status, target);
        issue.Status = target;
        issue.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}

public sealed record CreateIssueCommand(
    int ProjectId,
    string Title,
    string? Description,
    IssueStatus Status,
    IssuePriority Priority,
    IssueType Type,
    int? AssigneeId = null,
    int? SprintId = null,
    int StoryPoints = 0);
