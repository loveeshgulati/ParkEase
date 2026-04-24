namespace ParkEase.Payment.Entities;

public class Payment
{
    public int PaymentId { get; set; }
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public double Amount { get; set; }

    // PENDING | PAID | REFUNDED | FAILED
    public string Status { get; set; } = "PENDING";

    // CARD | UPI | WALLET | CASH
    public string Mode { get; set; } = string.Empty;

    // Razorpay-specific fields (replaces TransactionId)
    public string? RazorpayOrderId { get; set; }
    public string? RazorpayPaymentId { get; set; }

    public string Currency { get; set; } = "INR";
    public string? Description { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public double? RefundAmount { get; set; }
    public string? RefundReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public override string ToString() =>
        $"Payment[{PaymentId}] Booking={BookingId} Amount={Amount} Status={Status}";
}
