namespace ParkEase.Payment.DTOs;

/// <summary>
/// DTO representing a payment entity
/// </summary>
public class PaymentDto
{
    public int PaymentId { get; set; }
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public double Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public double? RefundAmount { get; set; }
    public string? RefundReason { get; set; }
    public DateTime CreatedAt { get; set; }
}
