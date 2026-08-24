using JiraClone.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiraClone.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class AuthController(AuthService auth) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Email and password are required." });
        var result = await auth.AuthenticateAsync(request.Email, request.Password, ct);
        return result is null
            ? Unauthorized(new { message = "Invalid credentials." })
            : Ok(new LoginResponse(result.Token, result.User));
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<AuthUser> Me() => Ok(new AuthUser(
        int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value),
        User.Identity?.Name ?? "",
        User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "",
        "",
        User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "User"));
}

public sealed record LoginRequest(string Email, string Password);
public sealed record LoginResponse(string Token, AuthUser User);
