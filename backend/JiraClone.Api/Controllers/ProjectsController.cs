using JiraClone.Api.Data;
using JiraClone.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class ProjectsController(JiraDbContext db, ProjectAuthorizationService authorization) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectResponse>>> Get(CancellationToken ct)
    {
        var ids = await authorization.GetVisibleProjectIdsAsync(ct);
        var projects = await db.Projects.AsNoTracking().Where(x => ids.Contains(x.Id)).OrderBy(x => x.Name).Select(x => new ProjectResponse(x.Id, x.Key, x.Name, x.Description, x.Issues.Count)).ToListAsync(ct);
        return Ok(projects);
    }
}

public sealed record ProjectResponse(int Id, string Key, string Name, string Description, int IssueCount);
