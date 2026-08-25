using JiraClone.Api.Data;
using JiraClone.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Services;

public sealed class ProjectDashboardService(JiraDbContext db)
{
    public async Task<ProjectDashboardResponse?> GetAsync(int projectId, CancellationToken ct)
    {
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == projectId, ct);
        if (project is null) return null;

        var issues = db.Issues.AsNoTracking().Where(x => x.ProjectId == projectId);
        var totalIssues = await issues.CountAsync(ct);
        var completedIssues = await issues.CountAsync(x => x.Status == IssueStatus.Done, ct);
        var openIssues = totalIssues - completedIssues;
        var totalPoints = await issues.SumAsync(x => (int?)x.StoryPoints, ct) ?? 0;
        var completedPoints = await issues.Where(x => x.Status == IssueStatus.Done).SumAsync(x => (int?)x.StoryPoints, ct) ?? 0;
        var distribution = await issues.GroupBy(x => x.Status).Select(g => new StatusCount(g.Key.ToString(), g.Count(), g.Sum(x => x.StoryPoints))).ToListAsync(ct);
        var velocity = await db.Sprints.AsNoTracking().Where(s => s.ProjectId == projectId && s.Status == SprintStatus.Completed).OrderByDescending(s => s.EndDate).Take(6).Select(s => new { s.Id, s.Name }).ToListAsync(ct);
        var velocityIds = velocity.Select(x => x.Id).ToList();
        var velocityRows = await db.Issues.AsNoTracking().Where(x => x.ProjectId == projectId && x.SprintId.HasValue && velocityIds.Contains(x.SprintId.Value) && x.Status == IssueStatus.Done).GroupBy(x => x.SprintId).Select(g => new { SprintId = g.Key!.Value, Points = g.Sum(x => x.StoryPoints) }).ToListAsync(ct);
        var active = await db.Sprints.AsNoTracking().Where(s => s.ProjectId == projectId && s.Status == SprintStatus.Active).Select(s => new { s.Id, s.Name, s.StartDate, s.EndDate }).FirstOrDefaultAsync(ct);
        var activeSummary = active is null ? null : new ActiveSprintSummary(active.Id, active.Name, active.StartDate, active.EndDate, await issues.CountAsync(x => x.SprintId == active.Id, ct), await issues.Where(x => x.SprintId == active.Id).SumAsync(x => (int?)x.StoryPoints, ct) ?? 0, await issues.Where(x => x.SprintId == active.Id && x.Status == IssueStatus.Done).SumAsync(x => (int?)x.StoryPoints, ct) ?? 0);
        return new ProjectDashboardResponse(project.Id, project.Key, project.Name, totalIssues, completedIssues, openIssues, totalPoints, completedPoints, distribution, velocity.Select(s => new VelocityPoint(s.Id, s.Name, velocityRows.FirstOrDefault(v => v.SprintId == s.Id)?.Points ?? 0)).Reverse().ToList(), activeSummary);
    }
}

public sealed record StatusCount(string Status, int Issues, int StoryPoints);
public sealed record VelocityPoint(int SprintId, string SprintName, int CompletedPoints);
public sealed record ActiveSprintSummary(int SprintId, string Name, DateTime StartDate, DateTime EndDate, int IssueCount, int CommittedPoints, int CompletedPoints);
public sealed record ProjectDashboardResponse(int ProjectId, string ProjectKey, string ProjectName, int TotalIssues, int CompletedIssues, int OpenIssues, int TotalStoryPoints, int CompletedStoryPoints, IReadOnlyList<StatusCount> StatusDistribution, IReadOnlyList<VelocityPoint> Velocity, ActiveSprintSummary? ActiveSprint);
