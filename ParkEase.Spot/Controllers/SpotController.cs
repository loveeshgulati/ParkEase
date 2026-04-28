using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkEase.Spot.DTOs.Request;
using ParkEase.Spot.DTOs.Response;
using ParkEase.Spot.DTOs.Common;
using ParkEase.Spot.Interfaces;

namespace ParkEase.Spot.Controllers;

[ApiController]
[Route("api/v1/spots")]
[Produces("application/json")]
public class SpotController : ControllerBase
{
    private readonly ISpotService _spotService;
    private readonly ILogger<SpotController> _logger;

    public SpotController(ISpotService spotService, ILogger<SpotController> logger)
    {
        _spotService = spotService;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC — No auth required
    // ═══════════════════════════════════════════════════════════════════════════

    // GET /api/v1/spots/lot/{lotId}
    [HttpGet("lot/{lotId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSpotsByLot(int lotId)
    {
        var result = await _spotService.GetSpotsByLotAsync(lotId);
        return Ok(ApiResponse<List<SpotDto>>.Ok(result,
            $"{result.Count} spots found"));
    }

    // GET /api/v1/spots/lot/{lotId}/available
    [HttpGet("lot/{lotId:int}/available")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableSpots(int lotId)
    {
        var result = await _spotService.GetAvailableSpotsByLotAsync(lotId);
        return Ok(ApiResponse<List<SpotDto>>.Ok(result,
            $"{result.Count} available spots"));
    }

    // GET /api/v1/spots/lot/{lotId}/count
    [HttpGet("lot/{lotId:int}/count")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableCount(int lotId)
    {
        var count = await _spotService.CountAvailableAsync(lotId);
        return Ok(ApiResponse<int>.Ok(count, $"{count} available spots"));
    }

    // GET /api/v1/spots/lot/{lotId}/type?spotType=STANDARD
    [HttpGet("lot/{lotId:int}/type")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSpotsByType(int lotId, [FromQuery] string spotType)
    {
        var result = await _spotService.GetSpotsByTypeAndLotAsync(lotId, spotType);
        return Ok(ApiResponse<List<SpotDto>>.Ok(result,
            $"{result.Count} {spotType} spots"));
    }

    // GET /api/v1/spots/lot/{lotId}/vehicle?vehicleType=4W
    [HttpGet("lot/{lotId:int}/vehicle")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSpotsByVehicleType(
        int lotId, [FromQuery] string vehicleType)
    {
        var result = await _spotService.GetSpotsByVehicleTypeAsync(lotId, vehicleType);
        return Ok(ApiResponse<List<SpotDto>>.Ok(result,
            $"{result.Count} spots for {vehicleType}"));
    }

    // GET /api/v1/spots/lot/{lotId}/floor/{floor}
    [HttpGet("lot/{lotId:int}/floor/{floor:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSpotsByFloor(int lotId, int floor)
    {
        var result = await _spotService.GetSpotsByFloorAsync(lotId, floor);
        return Ok(ApiResponse<List<SpotDto>>.Ok(result,
            $"{result.Count} spots on floor {floor}"));
    }

    // GET /api/v1/spots/lot/{lotId}/ev
    [HttpGet("lot/{lotId:int}/ev")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEVSpots(int lotId)
    {
        var result = await _spotService.GetEVSpotsByLotAsync(lotId);
        return Ok(ApiResponse<List<SpotDto>>.Ok(result,
            $"{result.Count} EV charging spots"));
    }

    // GET /api/v1/spots/lot/{lotId}/handicapped
    [HttpGet("lot/{lotId:int}/handicapped")]
    [AllowAnonymous]
    public async Task<IActionResult> GetHandicappedSpots(int lotId)
    {
        var result = await _spotService.GetHandicappedSpotsByLotAsync(lotId);
        return Ok(ApiResponse<List<SpotDto>>.Ok(result,
            $"{result.Count} handicapped spots"));
    }

    // GET /api/v1/spots/{id}
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSpotById(int id)
    {
        try
        {
            var result = await _spotService.GetSpotByIdAsync(id);
            return Ok(ApiResponse<SpotDto>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MANAGER — Manage spots in own lots
    // ═══════════════════════════════════════════════════════════════════════════

    // POST /api/v1/spots
    [HttpPost]
    [Authorize(Roles = "MANAGER,ADMIN")]
    public async Task<IActionResult> AddSpot([FromBody] AddSpotDto request)
    {
        try
        {
            var result = await _spotService.AddSpotAsync(GetCurrentUserId(), request);
            return StatusCode(201, ApiResponse<SpotDto>.Ok(result,
                "Spot added successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // POST /api/v1/spots/bulk
    [HttpPost("bulk")]
    [Authorize(Roles = "MANAGER,ADMIN")]
    public async Task<IActionResult> AddBulkSpots([FromBody] BulkAddSpotDto request)
    {
        try
        {
            var result = await _spotService.AddBulkSpotsAsync(GetCurrentUserId(), request);
            return StatusCode(201, ApiResponse<BulkAddResultDto>.Ok(result,
                $"{result.SpotsCreated} spots created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // PUT /api/v1/spots/{id}
    [HttpPut("{id:int}")]
    [Authorize(Roles = "MANAGER,ADMIN")]
    public async Task<IActionResult> UpdateSpot(int id, [FromBody] UpdateSpotDto request)
    {
        try
        {
            var result = await _spotService.UpdateSpotAsync(
                id, GetCurrentUserId(), request);
            return Ok(ApiResponse<SpotDto>.Ok(result, "Spot updated successfully"));
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

    // DELETE /api/v1/spots/{id}
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "MANAGER,ADMIN")]
    public async Task<IActionResult> DeleteSpot(int id)
    {
        try
        {
            await _spotService.DeleteSpotAsync(
                id, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(ApiResponse<object>.Ok(null!, "Spot deleted successfully"));
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

    // ═══════════════════════════════════════════════════════════════════════════
    // INTERNAL — Called by booking-service via HTTP
    // ═══════════════════════════════════════════════════════════════════════════

    // PUT /api/v1/spots/{id}/reserve
    [HttpPut("{id:int}/reserve")]
    [Authorize]
    public async Task<IActionResult> ReserveSpot(int id)
    {
        try
        {
            var result = await _spotService.ReserveSpotAsync(id);
            return Ok(ApiResponse<SpotDto>.Ok(result, "Spot reserved"));
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

    // PUT /api/v1/spots/{id}/occupy
    [HttpPut("{id:int}/occupy")]
    [Authorize]
    public async Task<IActionResult> OccupySpot(int id)
    {
        try
        {
            var result = await _spotService.OccupySpotAsync(id);
            return Ok(ApiResponse<SpotDto>.Ok(result, "Spot occupied"));
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

    // PUT /api/v1/spots/{id}/release
    [HttpPut("{id:int}/release")]
    [Authorize]
    public async Task<IActionResult> ReleaseSpot(int id)
    {
        try
        {
            var result = await _spotService.ReleaseSpotAsync(id);
            return Ok(ApiResponse<SpotDto>.Ok(result, "Spot released"));
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
