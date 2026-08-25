using JiraClone.Api.Data;
using JiraClone.Api.Models;
using JiraClone.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:int}/sprints")]
public sealed class SprintsController(JiraDbContext db, SprintApplicationService service, ProjectAuthorizationService authorization) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(int projectId, CancellationToken ct) { await authorization.EnsureCanViewAsync(projectId, ct); return Ok(await db.Sprints.AsNoTracking().Where(x => x.ProjectId == projectId).OrderByDescending(x => x.Id).ToListAsync(ct)); }
    [HttpPost]
    public async Task<IActionResult> Create(int projectId, CreateSprintRequest request, CancellationToken ct) { await authorization.EnsureCanManageAsync(projectId, ct); return Created("", await service.CreateAsync(projectId, request.Name, request.Goal, request.StartDate, request.EndDate, ct)); }
    [HttpPost("{sprintId:int}/start")]
    public async Task<IActionResult> Start(int projectId, int sprintId, CancellationToken ct) { await authorization.EnsureCanManageAsync(projectId, ct); return Ok(await service.StartAsync(sprintId, ct)); }
    [HttpPost("{sprintId:int}/complete")]
    public async Task<IActionResult> Complete(int projectId, int sprintId, CancellationToken ct) { await authorization.EnsureCanManageAsync(projectId, ct); return Ok(await service.CompleteAsync(sprintId, ct)); }
    [HttpPost("{sprintId:int}/issues/{issueId:int}")]
    public async Task<IActionResult> AssignIssue(int projectId, int sprintId, int issueId, CancellationToken ct) { await authorization.EnsureCanEditAsync(projectId, ct); await service.AssignIssueAsync(issueId, sprintId, ct); return NoContent(); }
    [HttpDelete("issues/{issueId:int}")]
    public async Task<IActionResult> RemoveIssue(int projectId, int issueId, CancellationToken ct) { await authorization.EnsureCanEditAsync(projectId, ct); await service.RemoveIssueAsync(issueId, ct); return NoContent(); }
}

public sealed record CreateSprintRequest(string Name, string? Goal, DateTime StartDate, DateTime EndDate);
