using JiraClone.Api.Data;
using JiraClone.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Services;

public sealed class SprintAnalyticsService(JiraDbContext db)
{
    public async Task<SprintAnalyticsResponse?> GetAsync(int projectId, int sprintId, CancellationToken ct)
    {
        var sprint = await db.Sprints.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sprintId && x.ProjectId == projectId, ct);
        if (sprint is null) return null;
        var issues = await db.Issues.AsNoTracking().Where(x => x.ProjectId == projectId && x.SprintId == sprintId).Select(x => new SprintIssueMetric(x.Id, x.Status, Math.Max(0, x.StoryPoints), x.CreatedAt, x.UpdatedAt)).ToListAsync(ct);
        var committed = issues.Sum(x => x.StoryPoints);
        var completed = issues.Where(x => x.Status == IssueStatus.Done).Sum(x => x.StoryPoints);
        var total = Math.Max(0, (sprint.EndDate.Date - sprint.StartDate.Date).Days) + 1;
        var today = DateTime.UtcNow.Date;
        var elapsed = Math.Clamp((today - sprint.StartDate.Date).Days + 1, 0, total);
        var ideal = Enumerable.Range(0, total).Select(day => new BurndownPoint(sprint.StartDate.Date.AddDays(day), committed == 0 ? 0 : Math.Round(committed * (1d - (double)day / Math.Max(1, total - 1)), 1))).ToList();
        var actual = Enumerable.Range(0, elapsed).Select(day => { var date = sprint.StartDate.Date.AddDays(day); var remaining = issues.Where(x => x.UpdatedAt.Date <= date && x.Status != IssueStatus.Done).Sum(x => x.StoryPoints); return new BurndownPoint(date, remaining); }).ToList();
        return new SprintAnalyticsResponse(sprint.Id, sprint.Name, sprint.Status.ToString(), sprint.StartDate, sprint.EndDate, committed, completed, Math.Max(0, committed - completed), issues.Count, issues.Count(x => x.Status == IssueStatus.Done), ideal, actual);
    }
}

public sealed record SprintIssueMetric(int Id, IssueStatus Status, int StoryPoints, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record BurndownPoint(DateTime Date, double RemainingPoints);
public sealed record SprintAnalyticsResponse(int SprintId, string Name, string Status, DateTime StartDate, DateTime EndDate, int CommittedPoints, int CompletedPoints, int RemainingPoints, int IssueCount, int CompletedIssueCount, IReadOnlyList<BurndownPoint> Ideal, IReadOnlyList<BurndownPoint> Actual);
