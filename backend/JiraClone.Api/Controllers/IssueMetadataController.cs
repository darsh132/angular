using JiraClone.Api.Data;
using JiraClone.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Controllers;

[ApiController, Route("api")]
public sealed class IssueMetadataController(JiraDbContext db, IssueMetadataApplicationService service, ProjectAuthorizationService authorization) : ControllerBase
{
    [HttpGet("projects/{projectId:int}/labels")]
    public async Task<IActionResult> Labels(int projectId, CancellationToken ct) { await authorization.EnsureCanViewAsync(projectId, ct); return Ok(await db.IssueLabels.AsNoTracking().Where(x => x.ProjectId == projectId).OrderBy(x => x.Name).Select(x => new LabelResponse(x.Id, x.Name, x.Color)).ToListAsync(ct)); }
    [HttpPost("projects/{projectId:int}/labels")]
    public async Task<IActionResult> CreateLabel(int projectId, CreateLabelRequest request, CancellationToken ct) { await authorization.EnsureCanEditAsync(projectId, ct); try { var x = await service.CreateLabelAsync(projectId, request.Name, request.Color, ct); return Created($"/api/projects/{projectId}/labels/{x.Id}", new LabelResponse(x.Id, x.Name, x.Color)); } catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); } }
    [HttpGet("projects/{projectId:int}/components")]
    public async Task<IActionResult> Components(int projectId, CancellationToken ct) { await authorization.EnsureCanViewAsync(projectId, ct); return Ok(await db.IssueComponents.AsNoTracking().Where(x => x.ProjectId == projectId).OrderBy(x => x.Name).Select(x => new ComponentResponse(x.Id, x.Name, x.Description)).ToListAsync(ct)); }
    [HttpPost("projects/{projectId:int}/components")]
    public async Task<IActionResult> CreateComponent(int projectId, CreateComponentRequest request, CancellationToken ct) { await authorization.EnsureCanEditAsync(projectId, ct); try { var x = await service.CreateComponentAsync(projectId, request.Name, request.Description, ct); return Created($"/api/projects/{projectId}/components/{x.Id}", new ComponentResponse(x.Id, x.Name, x.Description)); } catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); } }
    [HttpPost("issues/{issueId:int}/labels/{labelId:int}")]
    public Task<IActionResult> AddLabel(int issueId, int labelId, CancellationToken ct) => ExecuteIssueEdit(issueId, () => service.AddLabelAsync(issueId, labelId, ct));
    [HttpDelete("issues/{issueId:int}/labels/{labelId:int}")]
    public Task<IActionResult> RemoveLabel(int issueId, int labelId, CancellationToken ct) => ExecuteIssueEdit(issueId, () => service.RemoveLabelAsync(issueId, labelId, ct));
    [HttpPost("issues/{issueId:int}/components/{componentId:int}")]
    public Task<IActionResult> AddComponent(int issueId, int componentId, CancellationToken ct) => ExecuteIssueEdit(issueId, () => service.AddComponentAsync(issueId, componentId, ct));
    [HttpDelete("issues/{issueId:int}/components/{componentId:int}")]
    public Task<IActionResult> RemoveComponent(int issueId, int componentId, CancellationToken ct) => ExecuteIssueEdit(issueId, () => service.RemoveComponentAsync(issueId, componentId, ct));
    [HttpPost("issues/{issueId:int}/watchers/{userId:int}")]
    public Task<IActionResult> AddWatcher(int issueId, int userId, CancellationToken ct) => ExecuteIssueEdit(issueId, () => service.AddWatcherAsync(issueId, userId, ct));
    [HttpDelete("issues/{issueId:int}/watchers/{userId:int}")]
    public Task<IActionResult> RemoveWatcher(int issueId, int userId, CancellationToken ct) => ExecuteIssueEdit(issueId, () => service.RemoveWatcherAsync(issueId, userId, ct));
    [HttpPatch("issues/{issueId:int}/due-date")]
    public Task<IActionResult> DueDate(int issueId, DueDateRequest request, CancellationToken ct) => ExecuteIssueEdit(issueId, () => service.SetDueDateAsync(issueId, request.DueDate, ct));

    private async Task<IActionResult> ExecuteIssueEdit(int issueId, Func<Task> action)
    {
        var projectId = await db.Issues.Where(x => x.Id == issueId).Select(x => (int?)x.ProjectId).FirstOrDefaultAsync(HttpContext.RequestAborted);
        if (projectId is null) return NotFound(); await authorization.EnsureCanEditAsync(projectId.Value, HttpContext.RequestAborted);
        try { await action(); return NoContent(); } catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); } catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }
}
public sealed record LabelResponse(int Id, string Name, string? Color);
public sealed record ComponentResponse(int Id, string Name, string? Description);
public sealed record CreateLabelRequest(string Name, string? Color);
public sealed record CreateComponentRequest(string Name, string? Description);
public sealed record DueDateRequest(DateTime? DueDate);
