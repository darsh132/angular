using JiraClone.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class UsersController(JiraDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var users = await db.Users.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new UserOption(x.Id, x.Name, x.Avatar))
            .ToListAsync(ct);
        return Ok(users);
    }
}

public sealed record UserOption(int Id, string Name, string Avatar);
