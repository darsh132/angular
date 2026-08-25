using JiraClone.Api.Data;
using JiraClone.Api.Models;
using JiraClone.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:int}/members")]
public sealed class ProjectMembersController(JiraDbContext db, ProjectAuthorizationService authorization) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(int projectId, CancellationToken ct)
    {
        await authorization.EnsureCanViewAsync(projectId, ct);
        return Ok(await db.ProjectMembers.AsNoTracking().Where(x => x.ProjectId == projectId).Include(x => x.User).Select(x => new ProjectMemberResponse(x.UserId, x.User.Name, x.User.Email, x.User.Avatar, x.Role.ToString())).ToListAsync(ct));
    }

    [HttpPut("{userId:int}")]
    public async Task<IActionResult> Upsert(int projectId, int userId, ProjectMemberRequest request, CancellationToken ct)
    {
        await authorization.EnsureCanManageAsync(projectId, ct);
        if (!await db.Projects.AnyAsync(x => x.Id == projectId, ct) || !await db.Users.AnyAsync(x => x.Id == userId, ct)) return NotFound();
        var member = await db.ProjectMembers.FirstOrDefaultAsync(x => x.ProjectId == projectId && x.UserId == userId, ct);
        if (member is null) { member = new ProjectMember { ProjectId = projectId, UserId = userId, Role = request.Role }; db.ProjectMembers.Add(member); }
        else member.Role = request.Role;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{userId:int}")]
    public async Task<IActionResult> Remove(int projectId, int userId, CancellationToken ct)
    {
        await authorization.EnsureCanManageAsync(projectId, ct);
        var member = await db.ProjectMembers.FirstOrDefaultAsync(x => x.ProjectId == projectId && x.UserId == userId, ct);
        if (member is null) return NotFound();
        db.ProjectMembers.Remove(member); await db.SaveChangesAsync(ct); return NoContent();
    }
}

public sealed record ProjectMemberResponse(int UserId, string Name, string Email, string Avatar, string Role);
public sealed record ProjectMemberRequest(ProjectRole Role);
