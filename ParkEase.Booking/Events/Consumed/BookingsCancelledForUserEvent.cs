namespace ParkEase.Booking.Events.Consumed;

/// <summary>
/// Response to saga after bookings are cancelled.
/// </summary>
public class BookingsCancelledForUserEvent
{
    public Guid SagaCorrelationId { get; set; }
    public int UserId { get; set; }
    public int CancelledCount { get; set; }
}
