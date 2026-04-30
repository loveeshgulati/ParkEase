namespace ParkEase.Payment.Events;

/// <summary>
/// Event consumed from Booking-Service when booking is cancelled
/// Used to process refund if applicable
/// </summary>
public class BookingCancelledEvent
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public int LotId { get; set; }
    public int SpotId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsEligibleForRefund { get; set; }
    public double RefundAmount { get; set; }
    public DateTime CancelledAt { get; set; }
}
