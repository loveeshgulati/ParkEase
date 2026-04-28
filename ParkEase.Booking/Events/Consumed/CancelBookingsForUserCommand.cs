namespace ParkEase.Booking.Events.Consumed;

/// <summary>
/// When admin deactivates a user account,
/// booking-service cancels their active bookings.
/// Part of AccountDeactivationSaga.
/// </summary>
public class CancelBookingsForUserCommand
{
    public Guid CorrelationId { get; set; }
    public int UserId { get; set; }
}
