using JiraClone.Api.Data;
using JiraClone.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Controllers;
[ApiController, Route("api/[controller]")]
public sealed class IssuesController(JiraDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? status, [FromQuery] string? search)
    {
        var query = db.Issues.AsNoTracking().Include(x => x.Assignee).Include(x => x.Project).AsQueryable();
        if (Enum.TryParse<IssueStatus>(status, true, out var s)) query = query.Where(x => x.Status == s);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Title.Contains(search) || x.Description.Contains(search));
        return Ok(await query.OrderByDescending(x => x.UpdatedAt).Select(x => new { x.Id, key = x.Project.Key + "-" + x.Number, x.Title, x.Description, status = x.Status.ToString(), priority = x.Priority.ToString(), type = x.Type.ToString(), assignee = x.Assignee == null ? null : new { x.Assignee.Id, x.Assignee.Name, x.Assignee.Avatar }, x.UpdatedAt }).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateIssueRequest request)
    {
        var project = await db.Projects.FirstAsync();
        var number = (await db.Issues.Where(x => x.ProjectId == project.Id).MaxAsync(x => (int?)x.Number) ?? 100) + 1;
        var issue = new Issue { ProjectId = project.Id, Number = number, Title = request.Title.Trim(), Description = request.Description ?? "", Status = request.Status, Priority = request.Priority, Type = request.Type, AssigneeId = request.AssigneeId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Issues.Add(issue); await db.SaveChangesAsync(); return Ok(issue.Id);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateStatusRequest request)
    { var issue = await db.Issues.FindAsync(id); if (issue is null) return NotFound(); issue.Status = request.Status; issue.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync(); return NoContent(); }
}
public record CreateIssueRequest(string Title, string? Description, IssueStatus Status = IssueStatus.Todo, IssuePriority Priority = IssuePriority.Medium, IssueType Type = IssueType.Task, int? AssigneeId = null);
public record UpdateStatusRequest(IssueStatus Status);
