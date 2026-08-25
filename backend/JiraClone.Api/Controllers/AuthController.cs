using JiraClone.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiraClone.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class AuthController(AuthService auth, IConfiguration configuration) : ControllerBase
{
    private const string RefreshCookie = "jira_refresh";

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Email and password are required." });
        var result = await auth.AuthenticateAsync(request.Email, request.Password, ct);
        if (result is null) return Unauthorized(new { message = "Invalid credentials." });
        SetRefreshCookie(result.RefreshToken);
        return Ok(new LoginResponse(result.Token, result.User));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> Refresh(CancellationToken ct)
    {
        if (!Request.Cookies.TryGetValue(RefreshCookie, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(new { message = "Refresh token is missing." });
        var result = await auth.RefreshAsync(refreshToken, ct);
        if (result is null) { Response.Cookies.Delete(RefreshCookie, CookieOptions()); return Unauthorized(new { message = "Invalid or expired refresh token." }); }
        SetRefreshCookie(result.RefreshToken);
        return Ok(new LoginResponse(result.Token, result.User));
    }

    [AllowAnonymous]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(CancellationToken ct)
    {
        if (Request.Cookies.TryGetValue(RefreshCookie, out var refreshToken) && !string.IsNullOrWhiteSpace(refreshToken)) await auth.RevokeAsync(refreshToken, ct);
        Response.Cookies.Delete(RefreshCookie, CookieOptions());
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<AuthUser> Me() => Ok(new AuthUser(
        int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value), User.Identity?.Name ?? "", User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "", "", User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "User"));

    private void SetRefreshCookie(string token) => Response.Cookies.Append(RefreshCookie, token, CookieOptions());
    private CookieOptions CookieOptions() => new() { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, IsEssential = true, Expires = DateTimeOffset.UtcNow.AddDays(Math.Clamp(configuration.GetValue("Jwt:RefreshTokenDays", 30), 1, 90)), Path = "/api/auth" };
}

public sealed record LoginRequest(string Email, string Password);
public sealed record LoginResponse(string Token, AuthUser User);
