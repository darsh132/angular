using System.Security.Claims;
using JiraClone.Api.Data;
using JiraClone.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Services;

public sealed class ProjectAuthorizationService(JiraDbContext db, IHttpContextAccessor http)
{
    public async Task<ProjectRole?> GetRoleAsync(int projectId, CancellationToken ct)
    {
        if (http.HttpContext?.User.IsInRole("Admin") == true) return ProjectRole.Manager;
        var id = http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(id, out var userId)) return null;
        return await db.ProjectMembers.Where(x => x.ProjectId == projectId && x.UserId == userId).Select(x => (ProjectRole?)x.Role).FirstOrDefaultAsync(ct);
    }
    public async Task EnsureCanViewAsync(int projectId, CancellationToken ct) => await EnsureAsync(projectId, ProjectRole.Viewer, ct);
    public async Task EnsureCanEditAsync(int projectId, CancellationToken ct) => await EnsureAsync(projectId, ProjectRole.Member, ct);
    public async Task EnsureCanManageAsync(int projectId, CancellationToken ct) => await EnsureAsync(projectId, ProjectRole.Manager, ct);
    private async Task EnsureAsync(int projectId, ProjectRole required, CancellationToken ct)
    {
        var role = await GetRoleAsync(projectId, ct);
        if (role is null || role.Value < required) throw new UnauthorizedAccessException("You do not have permission for this project.");
    }
}
