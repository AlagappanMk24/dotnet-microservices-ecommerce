using System.Security.Claims;
using AuthenticationApi.Application.Auth;
using AuthenticationApi.Application.Auth.Abstractions;
using AuthenticationApi.Application.Auth.Contracts;
using AuthenticationApi.Application.Auth.Exceptions;
using Ecommerce.SharedLibrary.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthenticationApi.Presentation.Controllers;

[ApiController]
[Route("api/v1/auth")]
[EnableRateLimiting("auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResult>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await authService.RegisterAsync(request, cancellationToken);
            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<AuthResult>.Ok(result, "Account created successfully.", HttpContext.TraceIdentifier));
        }
        catch (DuplicateAccountException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message, HttpContext.TraceIdentifier));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message, HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await authService.LoginAsync(request, cancellationToken);
            return Ok(ApiResponse<AuthResult>.Ok(result, "Login successful.", HttpContext.TraceIdentifier));
        }
        catch (InvalidCredentialsException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message, HttpContext.TraceIdentifier));
        }
        catch (AccountLockedException ex)
        {
            Response.Headers["Retry-After"] = Math.Max(1, (int)(ex.Until - DateTimeOffset.UtcNow).TotalSeconds).ToString();
            return StatusCode(StatusCodes.Status423Locked, ApiResponse<object>.Fail(ex.Message, HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await authService.RefreshAsync(request, cancellationToken);
            return Ok(ApiResponse<AuthResult>.Ok(result, "Token refreshed successfully.", HttpContext.TraceIdentifier));
        }
        catch (InvalidRefreshTokenException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message, HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var userId))
            return Unauthorized(ApiResponse<object>.Fail("Invalid access token.", HttpContext.TraceIdentifier));

        await authService.RevokeAsync(userId, cancellationToken);

        return Ok(ApiResponse<object>.Ok(new { }, "Refresh token revoked.", HttpContext.TraceIdentifier));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var userId))
            return Unauthorized(ApiResponse<object>.Fail("Invalid access token.", HttpContext.TraceIdentifier));

        var profile = await authService.GetProfileAsync(userId, cancellationToken);

        return profile is null
            ? NotFound(ApiResponse<object>.Fail("Account not found.", HttpContext.TraceIdentifier))
            : Ok(ApiResponse<UserProfile>.Ok(profile, "Profile retrieved successfully.", HttpContext.TraceIdentifier));
    }
}