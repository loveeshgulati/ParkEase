using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkEase.Vehicle.DTOs;
using ParkEase.Vehicle.Interfaces;

namespace ParkEase.Vehicle.Controllers;

[ApiController]
[Route("api/v1/vehicles")]
[Authorize]
[Produces("application/json")]
public class VehicleController : ControllerBase
{
    private readonly IVehicleService _vehicleService;
    private readonly ILogger<VehicleController> _logger;

    public VehicleController(IVehicleService vehicleService, ILogger<VehicleController> logger)
    {
        _vehicleService = vehicleService;
        _logger = logger;
    }

    // ── POST /api/v1/vehicles ─────────────────────────────────────────────────
    /// <summary>Register a new vehicle (Driver only)</summary>
    [HttpPost]
    [Authorize(Roles = "DRIVER")]
    public async Task<IActionResult> RegisterVehicle([FromBody] RegisterVehicleDto request)
    {
        try
        {
            var result = await _vehicleService.RegisterVehicleAsync(
                GetCurrentUserId(), request);
            return StatusCode(201, ApiResponse<VehicleDto>.Ok(result,
                "Vehicle registered successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // ── GET /api/v1/vehicles/my-vehicles ──────────────────────────────────────
    /// <summary>Get all vehicles for current driver</summary>
    [HttpGet("my-vehicles")]
    [Authorize(Roles = "DRIVER")]
    public async Task<IActionResult> GetMyVehicles()
    {
        var result = await _vehicleService.GetVehiclesByOwnerAsync(
            GetCurrentUserId(), GetCurrentUserId(), GetCurrentUserRole());
        return Ok(ApiResponse<List<VehicleDto>>.Ok(result,
            $"{result.Count} vehicles found"));
    }

    // ── GET /api/v1/vehicles/{id} ─────────────────────────────────────────────
    /// <summary>Get vehicle by id (owner or admin)</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "DRIVER,ADMIN")]
    public async Task<IActionResult> GetVehicleById(int id)
    {
        try
        {
            var result = await _vehicleService.GetVehicleByIdAsync(
                id, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(ApiResponse<VehicleDto>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
    }

    // ── GET /api/v1/vehicles/owner/{ownerId} ──────────────────────────────────
    /// <summary>Get all vehicles by owner id (admin only)</summary>
    [HttpGet("owner/{ownerId:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetVehiclesByOwner(int ownerId)
    {
        var result = await _vehicleService.GetVehiclesByOwnerAsync(
            ownerId, GetCurrentUserId(), GetCurrentUserRole());
        return Ok(ApiResponse<List<VehicleDto>>.Ok(result,
            $"{result.Count} vehicles found"));
    }

    // ── GET /api/v1/vehicles/all ──────────────────────────────────────────────
    /// <summary>Get all vehicles on platform (admin only)</summary>
    [HttpGet("all")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetAllVehicles()
    {
        var result = await _vehicleService.GetAllVehiclesAsync();
        return Ok(ApiResponse<List<VehicleDto>>.Ok(result,
            $"{result.Count} vehicles found"));
    }

    // ── PUT /api/v1/vehicles/{id} ─────────────────────────────────────────────
    /// <summary>Update vehicle details (owner only)</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "DRIVER")]
    public async Task<IActionResult> UpdateVehicle(int id, [FromBody] UpdateVehicleDto request)
    {
        try
        {
            var result = await _vehicleService.UpdateVehicleAsync(
                id, GetCurrentUserId(), request);
            return Ok(ApiResponse<VehicleDto>.Ok(result, "Vehicle updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // ── DELETE /api/v1/vehicles/{id} ──────────────────────────────────────────
    /// <summary>Delete a vehicle (owner or admin)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "DRIVER,ADMIN")]
    public async Task<IActionResult> DeleteVehicle(int id)
    {
        try
        {
            await _vehicleService.DeleteVehicleAsync(
                id, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(ApiResponse<object>.Ok(null!, "Vehicle deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
    }

    // ── GET /api/v1/vehicles/{id}/type ────────────────────────────────────────
    /// <summary>Get vehicle type — used by booking-service internally</summary>
    [HttpGet("{id:int}/type")]
    [Authorize(Roles = "DRIVER,ADMIN")]
    public async Task<IActionResult> GetVehicleType(int id)
    {
        try
        {
            var type = await _vehicleService.GetVehicleTypeAsync(id);
            return Ok(ApiResponse<string>.Ok(type));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // ── GET /api/v1/vehicles/{id}/is-ev ──────────────────────────────────────
    /// <summary>Check if vehicle is EV — used by booking-service internally</summary>
    [HttpGet("{id:int}/is-ev")]
    [Authorize(Roles = "DRIVER,ADMIN")]
    public async Task<IActionResult> IsEV(int id)
    {
        try
        {
            var isEV = await _vehicleService.IsEVVehicleAsync(id);
            return Ok(ApiResponse<bool>.Ok(isEV));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("userId")?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token.");
        return int.Parse(claim);
    }

    private string GetCurrentUserRole() =>
        User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
}
