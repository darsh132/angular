using JiraClone.Api.Data;
using JiraClone.Api.Models;
using JiraClone.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class IssuesController(JiraDbContext db, IssueApplicationService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? status, [FromQuery] string? search, CancellationToken ct)
    {
        var query = db.Issues.AsNoTracking().Include(x => x.Assignee).Include(x => x.Project).AsQueryable();
        if (Enum.TryParse<IssueStatus>(status, true, out var parsedStatus)) query = query.Where(x => x.Status == parsedStatus);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Title.Contains(search) || x.Description.Contains(search) || (x.Project.Key + "-" + x.Number).Contains(search));
        return Ok(await query.OrderByDescending(x => x.UpdatedAt).Select(x => new IssueResponse(x.Id, x.Project.Key + "-" + x.Number, x.Title, x.Description, x.Status.ToString(), x.Priority.ToString(), x.Type.ToString(), x.StoryPoints, x.Assignee == null ? null : new UserSummary(x.Assignee.Id, x.Assignee.Name, x.Assignee.Avatar), x.UpdatedAt)).ToListAsync(ct));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var issue = await db.Issues.AsNoTracking()
            .Include(x => x.Project).Include(x => x.Assignee)
            .Include(x => x.Comments).ThenInclude(x => x.Author)
            .Include(x => x.Activities).ThenInclude(x => x.Actor)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (issue is null) return NotFound();
        return Ok(new IssueDetailsResponse(
            issue.Id, issue.Project.Key + "-" + issue.Number, issue.Title, issue.Description,
            issue.Status.ToString(), issue.Priority.ToString(), issue.Type.ToString(), issue.StoryPoints,
            issue.Assignee == null ? null : new UserSummary(issue.Assignee.Id, issue.Assignee.Name, issue.Assignee.Avatar),
            issue.UpdatedAt,
            issue.Comments.OrderBy(x => x.CreatedAt).Select(x => new CommentResponse(x.Id, x.Body, x.Author.Name, x.Author.Avatar, x.CreatedAt)).ToList(),
            issue.Activities.OrderByDescending(x => x.CreatedAt).Select(x => new ActivityResponse(x.Id, x.Type.ToString(), x.OldValue, x.NewValue, x.Actor.Name, x.Actor.Avatar, x.CreatedAt)).ToList()));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateIssueRequest request, CancellationToken ct)
    {
        var issue = await service.CreateAsync(new CreateIssueCommand(
            request.ProjectId, request.Title, request.Description, request.Status, request.Priority,
            request.Type, request.AssigneeId, request.SprintId, request.StoryPoints), ct);
        return Created($"/api/issues/{issue.Id}", issue.Id);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateStatusRequest request, CancellationToken ct)
    {
        await service.MoveAsync(id, request.Status, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/comments")]
    public async Task<IActionResult> AddComment(int id, CreateCommentRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Body)) return BadRequest(new { message = "Comment body is required." });
        if (!await db.Issues.AnyAsync(x => x.Id == id, ct)) return NotFound();
        var author = await db.Users.OrderBy(x => x.Id).FirstOrDefaultAsync(ct);
        if (author is null) return BadRequest(new { message = "No user exists." });
        var now = DateTime.UtcNow;
        var comment = new IssueComment { IssueId = id, AuthorId = author.Id, Body = request.Body.Trim(), CreatedAt = now };
        db.IssueComments.Add(comment);
        db.IssueActivities.Add(new IssueActivity { IssueId = id, ActorId = author.Id, Type = IssueActivityType.CommentAdded, NewValue = "Comment added", CreatedAt = now });
        await db.SaveChangesAsync(ct);
        return Created($"/api/issues/{id}", comment.Id);
    }
}

public sealed record IssueResponse(int Id, string Key, string Title, string Description, string Status, string Priority, string Type, int StoryPoints, UserSummary? Assignee, DateTime UpdatedAt);
public sealed record IssueDetailsResponse(int Id, string Key, string Title, string Description, string Status, string Priority, string Type, int StoryPoints, UserSummary? Assignee, DateTime UpdatedAt, IReadOnlyList<CommentResponse> Comments, IReadOnlyList<ActivityResponse> Activities);
public sealed record UserSummary(int Id, string Name, string Avatar);
public sealed record CommentResponse(int Id, string Body, string Author, string Avatar, DateTime CreatedAt);
public sealed record ActivityResponse(int Id, string Type, string? OldValue, string? NewValue, string Actor, string Avatar, DateTime CreatedAt);
public sealed record CreateIssueRequest(int ProjectId, string Title, string? Description, IssueStatus Status = IssueStatus.Todo, IssuePriority Priority = IssuePriority.Medium, IssueType Type = IssueType.Task, int? AssigneeId = null, int? SprintId = null, int StoryPoints = 0);
public sealed record UpdateStatusRequest(IssueStatus Status);
public sealed record CreateCommentRequest(string Body);
