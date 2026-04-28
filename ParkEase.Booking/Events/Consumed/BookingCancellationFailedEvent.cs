namespace ParkEase.Booking.Events.Consumed;

/// <summary>
/// Response to saga if cancellation fails.
/// </summary>
public class BookingCancellationFailedEvent
{
    public Guid SagaCorrelationId { get; set; }
    public int UserId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
