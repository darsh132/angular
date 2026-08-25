using JiraClone.Api.Data;
using JiraClone.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JiraClone.Api.Services;

public sealed class IssueMetadataApplicationService(JiraDbContext db, IHttpContextAccessor httpContextAccessor)
{
    private async Task<int> CurrentUserIdAsync(CancellationToken ct)
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(value, out var id) || !await db.Users.AnyAsync(x => x.Id == id, ct)) throw new UnauthorizedAccessException();
        return id;
    }

    public async Task<IssueLabel> CreateLabelAsync(int projectId, string name, string? color, CancellationToken ct)
    {
        var normalized = NormalizeName(name, "Label name"); await EnsureProjectAsync(projectId, ct);
        if (await db.IssueLabels.AnyAsync(x => x.ProjectId == projectId && x.Name == normalized, ct)) throw new InvalidOperationException("A label with this name already exists.");
        var label = new IssueLabel { ProjectId = projectId, Name = normalized, Color = NormalizeOptional(color) }; db.IssueLabels.Add(label); await db.SaveChangesAsync(ct); return label;
    }

    public async Task<IssueComponent> CreateComponentAsync(int projectId, string name, string? description, CancellationToken ct)
    {
        var normalized = NormalizeName(name, "Component name"); await EnsureProjectAsync(projectId, ct);
        if (await db.IssueComponents.AnyAsync(x => x.ProjectId == projectId && x.Name == normalized, ct)) throw new InvalidOperationException("A component with this name already exists.");
        var component = new IssueComponent { ProjectId = projectId, Name = normalized, Description = NormalizeOptional(description) }; db.IssueComponents.Add(component); await db.SaveChangesAsync(ct); return component;
    }

    public async Task AddLabelAsync(int issueId, int labelId, CancellationToken ct)
    {
        var issue = await db.Issues.AsNoTracking().FirstOrDefaultAsync(x => x.Id == issueId, ct) ?? throw new KeyNotFoundException("Issue not found.");
        var label = await db.IssueLabels.FirstOrDefaultAsync(x => x.Id == labelId, ct) ?? throw new KeyNotFoundException("Label not found."); EnsureSameProject(issue.ProjectId, label.ProjectId);
        if (await db.Issues.Where(x => x.Id == issueId).SelectMany(x => x.Labels).AnyAsync(x => x.Id == labelId, ct)) return;
        var actor = await CurrentUserIdAsync(ct); var now = DateTime.UtcNow; var tracked = await db.Issues.FindAsync([issueId], ct) ?? throw new KeyNotFoundException("Issue not found.");
        tracked.Labels.Add(label); tracked.UpdatedAt = now; db.IssueActivities.Add(new IssueActivity { IssueId = issueId, ActorId = actor, Type = IssueActivityType.LabelChanged, NewValue = $"Added label '{label.Name}'", CreatedAt = now }); await db.SaveChangesAsync(ct);
    }

    public async Task RemoveLabelAsync(int issueId, int labelId, CancellationToken ct)
    {
        var issue = await db.Issues.Include(x => x.Labels).FirstOrDefaultAsync(x => x.Id == issueId, ct) ?? throw new KeyNotFoundException("Issue not found.");
        var label = issue.Labels.FirstOrDefault(x => x.Id == labelId) ?? throw new KeyNotFoundException("Issue label not found."); var actor = await CurrentUserIdAsync(ct); var now = DateTime.UtcNow; issue.Labels.Remove(label); issue.UpdatedAt = now; db.IssueActivities.Add(new IssueActivity { IssueId = issueId, ActorId = actor, Type = IssueActivityType.LabelChanged, NewValue = $"Removed label '{label.Name}'", CreatedAt = now }); await db.SaveChangesAsync(ct);
    }

    public async Task AddComponentAsync(int issueId, int componentId, CancellationToken ct)
    {
        var issue = await db.Issues.AsNoTracking().FirstOrDefaultAsync(x => x.Id == issueId, ct) ?? throw new KeyNotFoundException("Issue not found.");
        var component = await db.IssueComponents.FirstOrDefaultAsync(x => x.Id == componentId, ct) ?? throw new KeyNotFoundException("Component not found."); EnsureSameProject(issue.ProjectId, component.ProjectId);
        if (await db.Issues.Where(x => x.Id == issueId).SelectMany(x => x.Components).AnyAsync(x => x.Id == componentId, ct)) return;
        var actor = await CurrentUserIdAsync(ct); var now = DateTime.UtcNow; var tracked = await db.Issues.FindAsync([issueId], ct) ?? throw new KeyNotFoundException("Issue not found."); tracked.Components.Add(component); tracked.UpdatedAt = now; db.IssueActivities.Add(new IssueActivity { IssueId = issueId, ActorId = actor, Type = IssueActivityType.ComponentChanged, NewValue = $"Added component '{component.Name}'", CreatedAt = now }); await db.SaveChangesAsync(ct);
    }

    public async Task RemoveComponentAsync(int issueId, int componentId, CancellationToken ct)
    {
        var issue = await db.Issues.Include(x => x.Components).FirstOrDefaultAsync(x => x.Id == issueId, ct) ?? throw new KeyNotFoundException("Issue not found.");
        var component = issue.Components.FirstOrDefault(x => x.Id == componentId) ?? throw new KeyNotFoundException("Issue component not found."); var actor = await CurrentUserIdAsync(ct); var now = DateTime.UtcNow; issue.Components.Remove(component); issue.UpdatedAt = now; db.IssueActivities.Add(new IssueActivity { IssueId = issueId, ActorId = actor, Type = IssueActivityType.ComponentChanged, NewValue = $"Removed component '{component.Name}'", CreatedAt = now }); await db.SaveChangesAsync(ct);
    }

    public async Task AddWatcherAsync(int issueId, int userId, CancellationToken ct)
    {
        var issue = await db.Issues.AsNoTracking().FirstOrDefaultAsync(x => x.Id == issueId, ct) ?? throw new KeyNotFoundException("Issue not found.");
        if (!await db.Users.AnyAsync(x => x.Id == userId, ct)) throw new KeyNotFoundException("User not found.");
        if (!await db.ProjectMembers.AnyAsync(x => x.ProjectId == issue.ProjectId && x.UserId == userId, ct)) throw new InvalidOperationException("Watcher must be a member of the issue project.");
        if (await db.IssueWatchers.AnyAsync(x => x.IssueId == issueId && x.UserId == userId, ct)) return;
        var actor = await CurrentUserIdAsync(ct); var now = DateTime.UtcNow; db.IssueWatchers.Add(new IssueWatcher { IssueId = issueId, UserId = userId, CreatedAt = now }); db.IssueActivities.Add(new IssueActivity { IssueId = issueId, ActorId = actor, Type = IssueActivityType.WatcherAdded, NewValue = userId.ToString(), CreatedAt = now }); await db.SaveChangesAsync(ct);
    }

    public async Task RemoveWatcherAsync(int issueId, int userId, CancellationToken ct)
    {
        var watcher = await db.IssueWatchers.FirstOrDefaultAsync(x => x.IssueId == issueId && x.UserId == userId, ct) ?? throw new KeyNotFoundException("Watcher not found."); var actor = await CurrentUserIdAsync(ct); db.IssueWatchers.Remove(watcher); db.IssueActivities.Add(new IssueActivity { IssueId = issueId, ActorId = actor, Type = IssueActivityType.WatcherRemoved, NewValue = userId.ToString(), CreatedAt = DateTime.UtcNow }); await db.SaveChangesAsync(ct);
    }

    public async Task SetDueDateAsync(int issueId, DateTime? dueDate, CancellationToken ct)
    {
        var issue = await db.Issues.FirstOrDefaultAsync(x => x.Id == issueId, ct) ?? throw new KeyNotFoundException("Issue not found."); var actor = await CurrentUserIdAsync(ct); var old = issue.DueDate; var now = DateTime.UtcNow; issue.DueDate = dueDate; issue.UpdatedAt = now; db.IssueActivities.Add(new IssueActivity { IssueId = issueId, ActorId = actor, Type = IssueActivityType.DueDateChanged, OldValue = old?.ToString("O"), NewValue = dueDate?.ToString("O"), CreatedAt = now }); await db.SaveChangesAsync(ct);
    }

    private async Task EnsureProjectAsync(int projectId, CancellationToken ct) { if (!await db.Projects.AnyAsync(x => x.Id == projectId, ct)) throw new KeyNotFoundException("Project not found."); }
    private static void EnsureSameProject(int issueProjectId, int metadataProjectId) { if (issueProjectId != metadataProjectId) throw new InvalidOperationException("Issue and metadata must belong to the same project."); }
    private static string NormalizeName(string value, string field) { if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{field} is required.", field); return value.Trim(); }
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
