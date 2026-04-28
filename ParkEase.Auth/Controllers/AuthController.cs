using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkEase.Auth.DTOs;
using ParkEase.Auth.Interfaces;

namespace ParkEase.Auth.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    // POST /api/v1/auth/register
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return StatusCode(201, ApiResponse<RegisterResponseDto>.Ok(result, result.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // POST /api/v1/auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(ApiResponse<LoginResponseDto>.Ok(result, "Login successful"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // POST /api/v1/auth/logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync(GetCurrentUserId());
        return Ok(ApiResponse<object>.Ok(null!, "Logged out successfully"));
    }

    // POST /api/v1/auth/refresh
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(ApiResponse<TokenResponseDto>.Ok(result, "Token refreshed"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // GET /api/v1/auth/profile
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _authService.GetUserByIdAsync(GetCurrentUserId());
        return Ok(ApiResponse<UserProfileDto>.Ok(result));
    }

    // PUT /api/v1/auth/profile
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
    {
        var result = await _authService.UpdateProfileAsync(GetCurrentUserId(), request);
        return Ok(ApiResponse<UserProfileDto>.Ok(result, "Profile updated successfully"));
    }

    // PUT /api/v1/auth/password
    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        try
        {
            await _authService.ChangePasswordAsync(GetCurrentUserId(), request);
            return Ok(ApiResponse<object>.Ok(null!, "Password changed successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // DELETE /api/v1/auth/deactivate
    [HttpDelete("deactivate")]
    [Authorize]
    public async Task<IActionResult> Deactivate()
    {
        await _authService.DeactivateAccountAsync(GetCurrentUserId());
        return Ok(ApiResponse<object>.Ok(null!,
            "Account deactivation initiated."));
    }

    // GET /api/v1/auth/validate
    [HttpGet("validate")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateToken(
        [FromHeader(Name = "Authorization")] string authorization)
    {
        var token = authorization?.Split(" ").Last();
        if (string.IsNullOrEmpty(token))
            return Ok(ApiResponse<bool>.Ok(false, "No token provided"));

        var isValid = await _authService.ValidateTokenAsync(token);
        return Ok(ApiResponse<bool>.Ok(isValid,
            isValid ? "Token is valid" : "Token is invalid"));
    }

    // ── Helper ────────────────────────────────────────────────────────────────
    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("userId")?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token.");
        return int.Parse(claim);
    }
}
