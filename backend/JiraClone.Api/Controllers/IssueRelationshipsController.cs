using JiraClone.Api.Models;
using JiraClone.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiraClone.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/issues/{issueId:int}/relationships")]
public sealed class IssueRelationshipsController(IssueRelationshipService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<IssueRelationship>> Create(int issueId, CreateRelationshipRequest request, CancellationToken ct)
    {
        try
        {
            return Ok(await service.CreateAsync(issueId, request.TargetIssueId, request.Type, ct));
        }
        catch (KeyNotFoundException e) { return NotFound(new { message = e.Message }); }
        catch (InvalidOperationException e) { return Conflict(new { message = e.Message }); }
    }

    [HttpDelete("{relationshipId:long}")]
    public async Task<IActionResult> Delete(long relationshipId, CancellationToken ct)
    {
        try
        {
            await service.DeleteAsync(relationshipId, ct);
            return NoContent();
        }
        catch (KeyNotFoundException e) { return NotFound(new { message = e.Message }); }
    }
}

public sealed record CreateRelationshipRequest(int TargetIssueId, IssueRelationshipType Type);
