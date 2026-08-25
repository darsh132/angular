using System.Security.Claims;
using JiraClone.Api.Data;
using JiraClone.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Services;

public sealed class ProjectAuthorizationService(JiraDbContext db, IHttpContextAccessor http)
{
    public int? GetCurrentUserId()
    {
        var id = http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(id, out var userId) ? userId : null;
    }

    public async Task<ProjectRole?> GetRoleAsync(int projectId, CancellationToken ct)
    {
        if (http.HttpContext?.User.IsInRole("Admin") == true) return ProjectRole.Manager;
        var userId = GetCurrentUserId();
        if (userId is null) return null;
        return await db.ProjectMembers.Where(x => x.ProjectId == projectId && x.UserId == userId).Select(x => (ProjectRole?)x.Role).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<int>> GetVisibleProjectIdsAsync(CancellationToken ct)
    {
        if (http.HttpContext?.User.IsInRole("Admin") == true) return await db.Projects.Select(x => x.Id).ToListAsync(ct);
        var userId = GetCurrentUserId();
        if (userId is null) return [];
        return await db.ProjectMembers.Where(x => x.UserId == userId).Select(x => x.ProjectId).ToListAsync(ct);
    }

    public Task EnsureCanViewAsync(int projectId, CancellationToken ct) => EnsureAsync(projectId, ProjectRole.Viewer, ct);
    public Task EnsureCanEditAsync(int projectId, CancellationToken ct) => EnsureAsync(projectId, ProjectRole.Member, ct);
    public Task EnsureCanManageAsync(int projectId, CancellationToken ct) => EnsureAsync(projectId, ProjectRole.Manager, ct);

    private async Task EnsureAsync(int projectId, ProjectRole required, CancellationToken ct)
    {
        var role = await GetRoleAsync(projectId, ct);
        if (role is null || role.Value < required) throw new UnauthorizedAccessException("You do not have permission for this project.");
    }
}
