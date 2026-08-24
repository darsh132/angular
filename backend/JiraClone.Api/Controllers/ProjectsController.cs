using JiraClone.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class ProjectsController(JiraDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectResponse>>> Get(CancellationToken ct)
    {
        var projects = await db.Projects.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ProjectResponse(x.Id, x.Key, x.Name, x.Description, x.Issues.Count))
            .ToListAsync(ct);
        return Ok(projects);
    }
}

public sealed record ProjectResponse(int Id, string Key, string Name, string Description, int IssueCount);
