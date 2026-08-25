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
        return result is null ? Unauthorized(new { message = "Invalid credentials." }) : Ok(new LoginResponse(result.Token, result.RefreshToken, result.User));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> Refresh(RefreshRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken)) return BadRequest(new { message = "Refresh token is required." });
        var result = await auth.RefreshAsync(request.RefreshToken, ct);
        return result is null ? Unauthorized(new { message = "Invalid or expired refresh token." }) : Ok(new LoginResponse(result.Token, result.RefreshToken, result.User));
    }

    [AllowAnonymous]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(RefreshRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken)) return BadRequest(new { message = "Refresh token is required." });
        await auth.RevokeAsync(request.RefreshToken, ct);
        return NoContent();
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
public sealed record RefreshRequest(string RefreshToken);
public sealed record LoginResponse(string Token, string RefreshToken, AuthUser User);
