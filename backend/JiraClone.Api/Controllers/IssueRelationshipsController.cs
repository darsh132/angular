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
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<IssueRelationshipDto>>> Get(int issueId, CancellationToken ct) => Ok(await service.GetForIssueAsync(issueId, ct));

    [HttpPost]
    public async Task<ActionResult<IssueRelationship>> Create(int issueId, CreateRelationshipRequest request, CancellationToken ct) => Ok(await service.CreateAsync(issueId, request.TargetIssueId, request.Type, ct));

    [HttpDelete("{relationshipId:long}")]
    public async Task<IActionResult> Delete(long relationshipId, CancellationToken ct) { await service.DeleteAsync(relationshipId, ct); return NoContent(); }
}

public sealed record CreateRelationshipRequest(int TargetIssueId, IssueRelationshipType Type);
