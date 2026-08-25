using JiraClone.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace JiraClone.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:int}/sprints/{sprintId:int}/analytics")]
public sealed class SprintAnalyticsController(SprintAnalyticsService analytics, ProjectAuthorizationService authorization) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(int projectId, int sprintId, CancellationToken ct)
    {
        await authorization.EnsureCanViewAsync(projectId, ct);
        var result = await analytics.GetAsync(projectId, sprintId, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
