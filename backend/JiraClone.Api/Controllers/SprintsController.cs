using JiraClone.Api.Data;
using JiraClone.Api.Models;
using JiraClone.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:int}/sprints")]
public sealed class SprintsController(JiraDbContext db, SprintApplicationService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(int projectId, CancellationToken ct) => Ok(await db.Sprints.AsNoTracking().Where(x => x.ProjectId == projectId).OrderByDescending(x => x.Id).ToListAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create(int projectId, CreateSprintRequest request, CancellationToken ct)
        => Created("", await service.CreateAsync(projectId, request.Name, request.Goal, request.StartDate, request.EndDate, ct));

    [HttpPost("{sprintId:int}/start")]
    public async Task<IActionResult> Start(int sprintId, CancellationToken ct) => Ok(await service.StartAsync(sprintId, ct));

    [HttpPost("{sprintId:int}/complete")]
    public async Task<IActionResult> Complete(int sprintId, CancellationToken ct) => Ok(await service.CompleteAsync(sprintId, ct));

    [HttpPost("{sprintId:int}/issues/{issueId:int}")]
    public async Task<IActionResult> AssignIssue(int sprintId, int issueId, CancellationToken ct) { await service.AssignIssueAsync(issueId, sprintId, ct); return NoContent(); }

    [HttpDelete("issues/{issueId:int}")]
    public async Task<IActionResult> RemoveIssue(int issueId, CancellationToken ct) { await service.RemoveIssueAsync(issueId, ct); return NoContent(); }
}

public sealed record CreateSprintRequest(string Name, string? Goal, DateTime StartDate, DateTime EndDate);
