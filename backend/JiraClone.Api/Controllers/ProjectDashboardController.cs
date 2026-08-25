using JiraClone.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace JiraClone.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:int}/dashboard")]
public sealed class ProjectDashboardController(ProjectDashboardService dashboard, ProjectAuthorizationService authorization) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(int projectId, CancellationToken ct)
    {
        await authorization.EnsureCanViewAsync(projectId, ct);
        var result = await dashboard.GetAsync(projectId, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
