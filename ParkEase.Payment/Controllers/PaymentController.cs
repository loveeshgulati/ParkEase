using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkEase.Payment.DTOs;
using ParkEase.Payment.Interfaces;

namespace ParkEase.Payment.Controllers;

[ApiController]
[Route("api/v1/payments")]
[Authorize]
[Produces("application/json")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentService paymentService,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    
    
    

    
    [HttpPost("create-order")]
    [Authorize(Roles = "DRIVER")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto request)
    {
        try
        {
            var result = await _paymentService.CreateOrderAsync(request);
            return Ok(ApiResponse<RazorpayOrderDto>.Ok(result,
                "Payment order created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    
    [HttpPost("process")]
    [Authorize(Roles = "DRIVER")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto request)
    {
        try
        {
            var result = await _paymentService.ProcessPaymentAsync(
                GetCurrentUserId(), request);
            return StatusCode(201, ApiResponse<PaymentDto>.Ok(result,
                $"Payment of ₹{result.Amount} processed successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    
    [HttpGet("my-payments")]
    [Authorize(Roles = "DRIVER")]
    public async Task<IActionResult> GetMyPayments()
    {
        var result = await _paymentService.GetMyPaymentsAsync(GetCurrentUserId());
        return Ok(ApiResponse<List<PaymentDto>>.Ok(result,
            $"{result.Count} payments found"));
    }

    
    [HttpGet("{id:int}")]
    [Authorize(Roles = "DRIVER,MANAGER,ADMIN")]
    public async Task<IActionResult> GetPaymentById(int id)
    {
        try
        {
            var result = await _paymentService.GetPaymentByIdAsync(
                id, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(ApiResponse<PaymentDto>.Ok(result));
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

    
    [HttpGet("booking/{bookingId:int}")]
    [Authorize(Roles = "DRIVER,MANAGER,ADMIN")]
    public async Task<IActionResult> GetPaymentByBooking(int bookingId)
    {
        try
        {
            var result = await _paymentService.GetPaymentByBookingIdAsync(
                bookingId, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(ApiResponse<PaymentDto>.Ok(result));
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

    
    [HttpPost("refund")]
    [Authorize(Roles = "DRIVER,ADMIN")]
    public async Task<IActionResult> RefundPayment([FromBody] RefundPaymentDto request)
    {
        try
        {
            var result = await _paymentService.RefundPaymentAsync(
                GetCurrentUserId(), GetCurrentUserRole(), request);
            return Ok(ApiResponse<PaymentDto>.Ok(result,
                $"Refund of ₹{result.RefundAmount} processed"));
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

    
    [HttpGet("{id:int}/receipt")]
    [Authorize(Roles = "DRIVER,ADMIN")]
    public async Task<IActionResult> GetReceipt(int id)
    {
        try
        {
            var receipt = await _paymentService.GenerateReceiptAsync(
                id, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(ApiResponse<string>.Ok(receipt, "Receipt generated"));
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

    
    
    

    
    [HttpGet("revenue/{lotId:int}")]
    [Authorize(Roles = "MANAGER,ADMIN")]
    public async Task<IActionResult> GetRevenueByLot(
        int lotId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = to ?? DateTime.UtcNow;

        var result = await _paymentService.GetRevenueByLotAsync(
            lotId, fromDate, toDate);
        return Ok(ApiResponse<RevenueDto>.Ok(result,
            $"Revenue: ₹{result.TotalRevenue}"));
    }

    
    
    

    
    [HttpGet("all")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetAllPayments()
    {
        var result = await _paymentService.GetAllPaymentsAsync();
        return Ok(ApiResponse<List<PaymentDto>>.Ok(result,
            $"{result.Count} total payments"));
    }

    
    [HttpGet("platform/revenue")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetPlatformRevenue(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = to ?? DateTime.UtcNow;

        var result = await _paymentService.GetPlatformRevenueAsync(fromDate, toDate);
        return Ok(ApiResponse<PlatformRevenueDto>.Ok(result,
            $"Platform revenue: ₹{result.TotalRevenue}"));
    }

    
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
