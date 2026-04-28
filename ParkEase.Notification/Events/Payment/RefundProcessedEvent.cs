namespace ParkEase.Payment.Events.Published;

public class RefundProcessedEvent
{
    public int PaymentId { get; set; }
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public double RefundAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime RefundedAt { get; set; }
}

