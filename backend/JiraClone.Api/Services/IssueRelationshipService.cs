using JiraClone.Api.Data;
using JiraClone.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JiraClone.Api.Services;

public sealed record IssueRelationshipDto(long Id, IssueRelationshipType Type, IssueSummaryDto SourceIssue, IssueSummaryDto TargetIssue, DateTime CreatedAt);
public sealed record IssueSummaryDto(int Id, string Key, string Title);

public sealed class IssueRelationshipService(JiraDbContext db, IHttpContextAccessor http, ProjectAuthorizationService authorization)
{
    public async Task<IReadOnlyList<IssueRelationshipDto>> GetForIssueAsync(int issueId, CancellationToken ct)
    {
        var issue = await db.Issues.AsNoTracking().FirstOrDefaultAsync(x => x.Id == issueId, ct) ?? throw new KeyNotFoundException("Issue not found.");
        await authorization.EnsureCanViewAsync(issue.ProjectId, ct);
        return await db.IssueRelationships.AsNoTracking()
            .Where(x => x.SourceIssueId == issueId || x.TargetIssueId == issueId)
            .Select(x => new IssueRelationshipDto(x.Id, x.Type,
                new IssueSummaryDto(x.SourceIssue.Id, x.SourceIssue.Key, x.SourceIssue.Title),
                new IssueSummaryDto(x.TargetIssue.Id, x.TargetIssue.Key, x.TargetIssue.Title), x.CreatedAt))
            .OrderBy(x => x.Type).ThenBy(x => x.Id).ToListAsync(ct);
    }

    public async Task<IssueRelationship> CreateAsync(int sourceId, int targetId, IssueRelationshipType type, CancellationToken ct)
    {
        if (sourceId == targetId) throw new InvalidOperationException("An issue cannot be related to itself.");
        var source = await db.Issues.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sourceId, ct) ?? throw new KeyNotFoundException("Source issue not found.");
        var target = await db.Issues.AsNoTracking().FirstOrDefaultAsync(x => x.Id == targetId, ct) ?? throw new KeyNotFoundException("Target issue not found.");
        if (source.ProjectId != target.ProjectId) throw new InvalidOperationException("Issues must belong to the same project.");
        await authorization.EnsureCanEditAsync(source.ProjectId, ct);
        var symmetric = type is IssueRelationshipType.RelatesTo or IssueRelationshipType.Duplicates;
        var existing = symmetric ? await db.IssueRelationships.FirstOrDefaultAsync(x => x.Type == type && ((x.SourceIssueId == sourceId && x.TargetIssueId == targetId) || (x.SourceIssueId == targetId && x.TargetIssueId == sourceId)), ct) : await db.IssueRelationships.FirstOrDefaultAsync(x => x.Type == type && x.SourceIssueId == sourceId && x.TargetIssueId == targetId, ct);
        if (existing is not null) return existing;
        var claim = http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claim, out var userId)) throw new UnauthorizedAccessException();
        var relationship = new IssueRelationship { SourceIssueId = sourceId, TargetIssueId = targetId, Type = type, CreatedAt = DateTime.UtcNow, CreatedById = userId };
        db.IssueRelationships.Add(relationship); await db.SaveChangesAsync(ct); return relationship;
    }

    public async Task DeleteAsync(long id, CancellationToken ct)
    {
        var relationship = await db.IssueRelationships.Include(x => x.SourceIssue).FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new KeyNotFoundException("Relationship not found.");
        await authorization.EnsureCanEditAsync(relationship.SourceIssue.ProjectId, ct); db.IssueRelationships.Remove(relationship); await db.SaveChangesAsync(ct);
    }
}
