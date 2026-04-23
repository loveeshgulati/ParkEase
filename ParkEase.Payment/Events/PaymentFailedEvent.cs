namespace ParkEase.Payment.Events;

/// <summary>
/// Event published when a payment fails
/// </summary>
public class PaymentFailedEvent
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public double Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
}
