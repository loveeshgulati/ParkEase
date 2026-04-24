using System.ComponentModel.DataAnnotations;

namespace ParkEase.Payment.DTOs;

/// <summary>
/// DTO for verifying and processing a Razorpay payment.
/// The client calls POST /api/v1/payments/process after the Razorpay checkout
/// widget succeeds, passing the three identifiers provided by Razorpay.
/// </summary>
public class ProcessPaymentDto
{
    [Required] public int BookingId { get; set; }

    /// <summary>Payment mode: CARD | UPI | WALLET | CASH</summary>
    [Required] public string Mode { get; set; } = string.Empty;

    [Required] public double Amount { get; set; }

    // ── Razorpay fields ───────────────────────────────────────────────────────

    /// <summary>Order ID returned by POST /api/v1/payments/create-order</summary>
    [Required] public string RazorpayOrderId { get; set; } = string.Empty;

    /// <summary>Payment ID returned by Razorpay checkout on success</summary>
    [Required] public string RazorpayPaymentId { get; set; } = string.Empty;

    /// <summary>HMAC-SHA256 signature returned by Razorpay checkout on success</summary>
    [Required] public string RazorpaySignature { get; set; } = string.Empty;

    public string? Description { get; set; }
}
