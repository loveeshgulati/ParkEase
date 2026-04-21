using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkEase.ParkingLot.DTOs;
using ParkEase.ParkingLot.Interfaces;

namespace ParkEase.ParkingLot.Controllers;

[ApiController]
[Route("api/v1/lots")]
[Produces("application/json")]
public class ParkingLotController : ControllerBase
{
    private readonly IParkingLotService _lotService;
    private readonly ILogger<ParkingLotController> _logger;

    public ParkingLotController(
        IParkingLotService lotService,
        ILogger<ParkingLotController> logger)
    {
        _lotService = lotService;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC — No auth required
    // ═══════════════════════════════════════════════════════════════════════════

    // GET /api/v1/lots/search?city=Delhi
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchByCity([FromQuery] string city)
    {
        try
        {
            var result = await _lotService.SearchLotsByCityAsync(city);
            return Ok(ApiResponse<List<LotDto>>.Ok(result,
                $"{result.Count} lots found in {city}"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // GET /api/v1/lots/nearby?lat=28.6&lng=77.2&radius=5
    [HttpGet("nearby")]
    [AllowAnonymous]
    public async Task<IActionResult> GetNearbyLots(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radius = 5.0)
    {
        var result = await _lotService.GetNearbyLotsAsync(lat, lng, radius);
        return Ok(ApiResponse<List<NearbyLotDto>>.Ok(result,
            $"{result.Count} lots found within {radius}km"));
    }

    // GET /api/v1/lots/{id}
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLotById(int id)
    {
        try
        {
            var result = await _lotService.GetLotByIdAsync(id);
            return Ok(ApiResponse<LotDto>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MANAGER — Manage own lots
    // ═══════════════════════════════════════════════════════════════════════════

    // POST /api/v1/lots
    [HttpPost]
    [Authorize(Roles = "MANAGER")]
    public async Task<IActionResult> CreateLot([FromBody] CreateLotDto request)
    {
        try
        {
            var result = await _lotService.CreateLotAsync(GetCurrentUserId(), request);
            return StatusCode(201, ApiResponse<LotDto>.Ok(result,
                "Lot registered successfully. Awaiting admin approval."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // GET /api/v1/lots/my-lots
    [HttpGet("my-lots")]
    [Authorize(Roles = "MANAGER")]
    public async Task<IActionResult> GetMyLots()
    {
        var result = await _lotService.GetLotsByManagerAsync(GetCurrentUserId());
        return Ok(ApiResponse<List<LotDto>>.Ok(result,
            $"{result.Count} lots found"));
    }

    // PUT /api/v1/lots/{id}
    [HttpPut("{id:int}")]
    [Authorize(Roles = "MANAGER,ADMIN")]
    public async Task<IActionResult> UpdateLot(int id, [FromBody] UpdateLotDto request)
    {
        try
        {
            var result = await _lotService.UpdateLotAsync(
                id, GetCurrentUserId(), request);
            return Ok(ApiResponse<LotDto>.Ok(result, "Lot updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    // DELETE /api/v1/lots/{id}
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "MANAGER,ADMIN")]
    public async Task<IActionResult> DeleteLot(int id)
    {
        try
        {
            await _lotService.DeleteLotAsync(
                id, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(ApiResponse<object>.Ok(null!, "Lot deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    // PUT /api/v1/lots/{id}/toggle
    [HttpPut("{id:int}/toggle")]
    [Authorize(Roles = "MANAGER,ADMIN")]
    public async Task<IActionResult> ToggleLot(int id)
    {
        try
        {
            var result = await _lotService.ToggleOpenAsync(
                id, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(ApiResponse<LotDto>.Ok(result,
                $"Lot is now {(result.IsOpen ? "OPEN" : "CLOSED")}"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ADMIN — Platform management
    // ═══════════════════════════════════════════════════════════════════════════

    // GET /api/v1/lots/all
    [HttpGet("all")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetAllLots()
    {
        var result = await _lotService.GetAllLotsAsync();
        return Ok(ApiResponse<List<LotDto>>.Ok(result,
            $"{result.Count} total lots"));
    }

    // GET /api/v1/lots/pending
    [HttpGet("pending")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetPendingLots()
    {
        var result = await _lotService.GetPendingLotsAsync();
        return Ok(ApiResponse<List<LotDto>>.Ok(result,
            $"{result.Count} pending lots awaiting approval"));
    }

    // PUT /api/v1/lots/{id}/approve
    [HttpPut("{id:int}/approve")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> ApproveLot(int id)
    {
        try
        {
            var result = await _lotService.ApproveLotAsync(id, GetCurrentUserId());
            return Ok(ApiResponse<LotDto>.Ok(result,
                $"Lot '{result.Name}' approved. Manager can now open it."));
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

    // PUT /api/v1/lots/{id}/reject
    [HttpPut("{id:int}/reject")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> RejectLot(int id, [FromBody] RejectLotDto request)
    {
        try
        {
            var result = await _lotService.RejectLotAsync(
                id, GetCurrentUserId(), request.Reason);
            return Ok(ApiResponse<LotDto>.Ok(result,
                $"Lot '{result.Name}' rejected."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // GET /api/v1/lots/manager/{managerId}
    [HttpGet("manager/{managerId:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetLotsByManager(int managerId)
    {
        var result = await _lotService.GetLotsByManagerAsync(managerId);
        return Ok(ApiResponse<List<LotDto>>.Ok(result,
            $"{result.Count} lots for manager {managerId}"));
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
