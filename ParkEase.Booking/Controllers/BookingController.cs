using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkEase.Booking.DTOs.Request;
using ParkEase.Booking.DTOs.Response;
using ParkEase.Booking.DTOs.Common;
using ParkEase.Booking.Interfaces;

namespace ParkEase.Booking.Controllers;

[ApiController]
[Route("api/v1/bookings")]
[Authorize]
[Produces("application/json")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingController> _logger;

    public BookingController(
        IBookingService bookingService,
        ILogger<BookingController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DRIVER
    // ═══════════════════════════════════════════════════════════════════════════

    // POST /api/v1/bookings
    [HttpPost]
    [Authorize(Roles = "DRIVER")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto request)
    {
        try
        {
            var result = await _bookingService.CreateBookingAsync(
                GetCurrentUserId(), request);
            return StatusCode(201, ApiResponse<BookingDto>.Ok(result,
                "Booking created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // GET /api/v1/bookings/my-bookings
    [HttpGet("my-bookings")]
    [Authorize(Roles = "DRIVER")]
    public async Task<IActionResult> GetMyBookings()
    {
        var result = await _bookingService.GetMyBookingsAsync(GetCurrentUserId());
        return Ok(ApiResponse<List<BookingDto>>.Ok(result,
            $"{result.Count} bookings found"));
    }

    // GET /api/v1/bookings/{id}
    [HttpGet("{id:int}")]
    [Authorize(Roles = "DRIVER,MANAGER,ADMIN")]
    public async Task<IActionResult> GetBookingById(int id)
    {
        try
        {
            var result = await _bookingService.GetBookingByIdAsync(
                id, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(ApiResponse<BookingDto>.Ok(result));
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

    // PUT /api/v1/bookings/{id}/cancel
    [HttpPut("{id:int}/cancel")]
    [Authorize(Roles = "DRIVER,MANAGER,ADMIN")]
    public async Task<IActionResult> CancelBooking(
        int id, [FromBody] CancelBookingDto request)
    {
        try
        {
            var result = await _bookingService.CancelBookingAsync(
                id, GetCurrentUserId(), GetCurrentUserRole(), request.Reason);
            return Ok(ApiResponse<BookingDto>.Ok(result,
                "Booking cancelled successfully"));
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

    // PUT /api/v1/bookings/{id}/checkin
    [HttpPut("{id:int}/checkin")]
    [Authorize(Roles = "DRIVER,MANAGER,ADMIN")]
    public async Task<IActionResult> CheckIn(int id)
    {
        try
        {
            var result = await _bookingService.CheckInAsync(
                id, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(ApiResponse<BookingDto>.Ok(result,
                "Checked in successfully"));
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

    // PUT /api/v1/bookings/{id}/checkout
    [HttpPut("{id:int}/checkout")]
    [Authorize(Roles = "DRIVER,MANAGER,ADMIN")]
    public async Task<IActionResult> CheckOut(int id)
    {
        try
        {
            var result = await _bookingService.CheckOutAsync(
                id, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(ApiResponse<BookingDto>.Ok(result,
                $"Checked out. Total: ₹{result.TotalAmount}"));
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

    // PUT /api/v1/bookings/{id}/extend
    [HttpPut("{id:int}/extend")]
    [Authorize(Roles = "DRIVER")]
    public async Task<IActionResult> ExtendBooking(
        int id, [FromBody] ExtendBookingDto request)
    {
        try
        {
            var result = await _bookingService.ExtendBookingAsync(
                id, GetCurrentUserId(), request);
            return Ok(ApiResponse<BookingDto>.Ok(result,
                "Booking extended successfully"));
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

    // GET /api/v1/bookings/{id}/fare
    [HttpGet("{id:int}/fare")]
    [Authorize(Roles = "DRIVER,MANAGER,ADMIN")]
    public async Task<IActionResult> CalculateFare(int id)
    {
        try
        {
            var result = await _bookingService.CalculateFareAsync(id);
            return Ok(ApiResponse<FareCalculationDto>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MANAGER
    // ═══════════════════════════════════════════════════════════════════════════

    // GET /api/v1/bookings/lot/{lotId}
    [HttpGet("lot/{lotId:int}")]
    [Authorize(Roles = "MANAGER,ADMIN")]
    public async Task<IActionResult> GetBookingsByLot(int lotId)
    {
        var result = await _bookingService.GetBookingsByLotAsync(
            lotId, GetCurrentUserId(), GetCurrentUserRole());
        return Ok(ApiResponse<List<BookingDto>>.Ok(result,
            $"{result.Count} bookings"));
    }

    // GET /api/v1/bookings/lot/{lotId}/active
    [HttpGet("lot/{lotId:int}/active")]
    [Authorize(Roles = "MANAGER,ADMIN")]
    public async Task<IActionResult> GetActiveBookingsByLot(int lotId)
    {
        var result = await _bookingService.GetActiveBookingsByLotAsync(
            lotId, GetCurrentUserId(), GetCurrentUserRole());
        return Ok(ApiResponse<List<BookingDto>>.Ok(result,
            $"{result.Count} active check-ins"));
    }

    // PUT /api/v1/bookings/{id}/force-checkout
    [HttpPut("{id:int}/force-checkout")]
    [Authorize(Roles = "MANAGER,ADMIN")]
    public async Task<IActionResult> ForceCheckOut(int id)
    {
        try
        {
            var result = await _bookingService.ForceCheckOutAsync(
                id, GetCurrentUserId());
            return Ok(ApiResponse<BookingDto>.Ok(result,
                $"Force checkout complete. Amount: ₹{result.TotalAmount}"));
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
    // ADMIN
    // ═══════════════════════════════════════════════════════════════════════════

    // GET /api/v1/bookings/all
    [HttpGet("all")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetAllBookings()
    {
        var result = await _bookingService.GetAllBookingsAsync();
        return Ok(ApiResponse<List<BookingDto>>.Ok(result,
            $"{result.Count} total bookings"));
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
