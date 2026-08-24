using JiraClone.Api.Data;
using JiraClone.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Services;

public sealed class SprintApplicationService(JiraDbContext db)
{
    public async Task<Sprint> CreateAsync(int projectId, string name, string? goal, DateTime startDate, DateTime endDate, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Sprint name is required.");
        if (endDate <= startDate) throw new ArgumentException("Sprint end date must be after its start date.");
        if (!await db.Projects.AnyAsync(x => x.Id == projectId, ct)) throw new KeyNotFoundException("Project not found.");

        var sprint = new Sprint { ProjectId = projectId, Name = name.Trim(), Goal = goal?.Trim(), StartDate = startDate, EndDate = endDate, Status = SprintStatus.Planned };
        db.Sprints.Add(sprint);
        await db.SaveChangesAsync(ct);
        return sprint;
    }

    public async Task<Sprint> StartAsync(int sprintId, CancellationToken ct)
    {
        var sprint = await db.Sprints.FirstOrDefaultAsync(x => x.Id == sprintId, ct) ?? throw new KeyNotFoundException("Sprint not found.");
        if (sprint.Status != SprintStatus.Planned) throw new InvalidOperationException("Only a planned sprint can be started.");
        if (await db.Sprints.AnyAsync(x => x.ProjectId == sprint.ProjectId && x.Status == SprintStatus.Active && x.Id != sprintId, ct))
            throw new InvalidOperationException("A project can have only one active sprint.");
        sprint.Status = SprintStatus.Active;
        await db.SaveChangesAsync(ct);
        return sprint;
    }

    public async Task<Sprint> CompleteAsync(int sprintId, CancellationToken ct)
    {
        var sprint = await db.Sprints.FirstOrDefaultAsync(x => x.Id == sprintId, ct) ?? throw new KeyNotFoundException("Sprint not found.");
        if (sprint.Status != SprintStatus.Active) throw new InvalidOperationException("Only an active sprint can be completed.");
        sprint.Status = SprintStatus.Completed;
        await db.SaveChangesAsync(ct);
        return sprint;
    }

    public async Task AssignIssueAsync(int issueId, int sprintId, CancellationToken ct)
    {
        var issue = await db.Issues.FirstOrDefaultAsync(x => x.Id == issueId, ct) ?? throw new KeyNotFoundException("Issue not found.");
        var sprint = await db.Sprints.FirstOrDefaultAsync(x => x.Id == sprintId, ct) ?? throw new KeyNotFoundException("Sprint not found.");
        if (issue.ProjectId != sprint.ProjectId) throw new InvalidOperationException("Issue and sprint must belong to the same project.");
        if (sprint.Status == SprintStatus.Completed) throw new InvalidOperationException("A completed sprint cannot receive issues.");
        issue.SprintId = sprintId;
        issue.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveIssueAsync(int issueId, CancellationToken ct)
    {
        var issue = await db.Issues.FirstOrDefaultAsync(x => x.Id == issueId, ct) ?? throw new KeyNotFoundException("Issue not found.");
        issue.SprintId = null;
        issue.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
