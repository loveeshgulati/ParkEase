namespace ParkEase.Payment.Events;

/// <summary>
/// Event published when a payment is successfully processed
/// </summary>
public class PaymentProcessedEvent
{
    public int PaymentId { get; set; }
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public double Amount { get; set; }
    public string Mode { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public DateTime PaidAt { get; set; }
}
