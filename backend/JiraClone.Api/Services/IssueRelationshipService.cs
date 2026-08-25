using JiraClone.Api.Data;
using JiraClone.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JiraClone.Api.Services;

public sealed class IssueRelationshipService(JiraDbContext db, IHttpContextAccessor http)
{
    public async Task<IssueRelationship> CreateAsync(int sourceId, int targetId, IssueRelationshipType type, CancellationToken ct)
    {
        if (sourceId == targetId) throw new InvalidOperationException("An issue cannot be related to itself.");
        var source = await db.Issues.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sourceId, ct) ?? throw new KeyNotFoundException("Source issue not found.");
        var target = await db.Issues.AsNoTracking().FirstOrDefaultAsync(x => x.Id == targetId, ct) ?? throw new KeyNotFoundException("Target issue not found.");
        if (source.ProjectId != target.ProjectId) throw new InvalidOperationException("Issues must belong to the same project.");

        var symmetric = type is IssueRelationshipType.RelatesTo or IssueRelationshipType.Duplicates;
        var existing = symmetric
            ? await db.IssueRelationships.FirstOrDefaultAsync(x => x.Type == type && ((x.SourceIssueId == sourceId && x.TargetIssueId == targetId) || (x.SourceIssueId == targetId && x.TargetIssueId == sourceId)), ct)
            : await db.IssueRelationships.FirstOrDefaultAsync(x => x.Type == type && x.SourceIssueId == sourceId && x.TargetIssueId == targetId, ct);
        if (existing is not null) return existing;

        var claim = http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claim, out var userId)) throw new UnauthorizedAccessException();
        var relationship = new IssueRelationship { SourceIssueId = sourceId, TargetIssueId = targetId, Type = type, CreatedAt = DateTime.UtcNow, CreatedById = userId };
        db.IssueRelationships.Add(relationship); await db.SaveChangesAsync(ct); return relationship;
    }

    public async Task DeleteAsync(long id, CancellationToken ct)
    {
        var relationship = await db.IssueRelationships.FindAsync([id], ct) ?? throw new KeyNotFoundException("Relationship not found.");
        db.IssueRelationships.Remove(relationship); await db.SaveChangesAsync(ct);
    }
}
