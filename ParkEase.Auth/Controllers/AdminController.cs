using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkEase.Auth.DTOs;
using ParkEase.Auth.Interfaces;

namespace ParkEase.Auth.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "ADMIN")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MANAGER MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    // GET /api/v1/admin/managers/pending
    [HttpGet("managers/pending")]
    public async Task<IActionResult> GetPendingManagers()
    {
        var result = await _adminService.GetPendingManagersAsync();
        return Ok(ApiResponse<List<PendingManagerDto>>.Ok(result,
            $"{result.Count} pending manager requests"));
    }

    // GET /api/v1/admin/managers
    [HttpGet("managers")]
    public async Task<IActionResult> GetAllManagers()
    {
        var result = await _adminService.GetAllManagersAsync();
        return Ok(ApiResponse<List<ManagerDto>>.Ok(result,
            $"{result.Count} managers found"));
    }

    // GET /api/v1/admin/managers/{id}
    [HttpGet("managers/{id:int}")]
    public async Task<IActionResult> GetManagerById(int id)
    {
        try
        {
            var result = await _adminService.GetManagerByIdAsync(id);
            return Ok(ApiResponse<UserProfileDto>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // PUT /api/v1/admin/managers/{id}/approve
    [HttpPut("managers/{id:int}/approve")]
    public async Task<IActionResult> ApproveManager(int id)
    {
        try
        {
            await _adminService.ApproveManagerAsync(id, GetCurrentAdminId());
            return Ok(ApiResponse<object>.Ok(null!,
                $"Manager {id} approved successfully. They can now login."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // PUT /api/v1/admin/managers/{id}/reject
    [HttpPut("managers/{id:int}/reject")]
    public async Task<IActionResult> RejectManager(int id, [FromBody] RejectManagerDto request)
    {
        try
        {
            await _adminService.RejectManagerAsync(id, GetCurrentAdminId(), request.Reason);
            return Ok(ApiResponse<object>.Ok(null!,
                $"Manager {id} rejected. Reason: {request.Reason}"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // PUT /api/v1/admin/managers/{id}/suspend
    [HttpPut("managers/{id:int}/suspend")]
    public async Task<IActionResult> SuspendManager(int id, [FromBody] SuspendUserDto request)
    {
        try
        {
            await _adminService.SuspendManagerAsync(id, GetCurrentAdminId(), request.Reason);
            return Ok(ApiResponse<object>.Ok(null!, $"Manager {id} suspended."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // PUT /api/v1/admin/managers/{id}/reactivate
    [HttpPut("managers/{id:int}/reactivate")]
    public async Task<IActionResult> ReactivateManager(int id)
    {
        try
        {
            await _adminService.ReactivateManagerAsync(id, GetCurrentAdminId());
            return Ok(ApiResponse<object>.Ok(null!, $"Manager {id} reactivated."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // DELETE /api/v1/admin/managers/{id}
    [HttpDelete("managers/{id:int}")]
    public async Task<IActionResult> DeleteManager(int id)
    {
        try
        {
            await _adminService.DeleteManagerAsync(id, GetCurrentAdminId());
            return Ok(ApiResponse<object>.Ok(null!, $"Manager {id} permanently deleted."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DRIVER MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    // GET /api/v1/admin/drivers
    [HttpGet("drivers")]
    public async Task<IActionResult> GetAllDrivers()
    {
        var result = await _adminService.GetAllDriversAsync();
        return Ok(ApiResponse<List<DriverDto>>.Ok(result,
            $"{result.Count} drivers found"));
    }

    // GET /api/v1/admin/drivers/{id}
    [HttpGet("drivers/{id:int}")]
    public async Task<IActionResult> GetDriverById(int id)
    {
        try
        {
            var result = await _adminService.GetDriverByIdAsync(id);
            return Ok(ApiResponse<UserProfileDto>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // PUT /api/v1/admin/drivers/{id}/suspend
    [HttpPut("drivers/{id:int}/suspend")]
    public async Task<IActionResult> SuspendDriver(int id, [FromBody] SuspendUserDto request)
    {
        try
        {
            await _adminService.SuspendDriverAsync(id, GetCurrentAdminId(), request.Reason);
            return Ok(ApiResponse<object>.Ok(null!, $"Driver {id} suspended."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // PUT /api/v1/admin/drivers/{id}/reactivate
    [HttpPut("drivers/{id:int}/reactivate")]
    public async Task<IActionResult> ReactivateDriver(int id)
    {
        try
        {
            await _adminService.ReactivateDriverAsync(id, GetCurrentAdminId());
            return Ok(ApiResponse<object>.Ok(null!, $"Driver {id} reactivated."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // DELETE /api/v1/admin/drivers/{id}
    [HttpDelete("drivers/{id:int}")]
    public async Task<IActionResult> DeleteDriver(int id)
    {
        try
        {
            await _adminService.DeleteDriverAsync(id, GetCurrentAdminId());
            return Ok(ApiResponse<object>.Ok(null!, $"Driver {id} permanently deleted."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PLATFORM OVERVIEW
    // ═══════════════════════════════════════════════════════════════════════════

    // GET /api/v1/admin/users
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _adminService.GetAllUsersAsync();
        return Ok(ApiResponse<List<UserProfileDto>>.Ok(result,
            $"{result.Count} total users"));
    }

    // GET /api/v1/admin/users/{id}
    [HttpGet("users/{id:int}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        try
        {
            var result = await _adminService.GetUserByIdAsync(id);
            return Ok(ApiResponse<UserProfileDto>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // ── Helper ────────────────────────────────────────────────────────────────
    private int GetCurrentAdminId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("userId")?.Value
            ?? throw new UnauthorizedAccessException("Admin ID not found in token.");
        return int.Parse(claim);
    }
}
