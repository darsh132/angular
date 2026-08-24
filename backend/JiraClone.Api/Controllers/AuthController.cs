using JiraClone.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class AuthController(JiraDbContext db) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Email and password are required." });

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == request.Email, ct);
        if (user is null)
            return Unauthorized(new { message = "Invalid credentials." });

        // Development-only authentication. Replace with ASP.NET Core Identity/JWT before production.
        return user.PasswordHash == request.Password
            ? Ok(new LoginResponse("development-token", new UserResponse(user.Id, user.Name, user.Email, user.Avatar)))
            : Unauthorized(new { message = "Invalid credentials." });
    }
}

public sealed record LoginRequest(string Email, string Password);
public sealed record LoginResponse(string Token, UserResponse User);
public sealed record UserResponse(int Id, string Name, string Email, string Avatar);
