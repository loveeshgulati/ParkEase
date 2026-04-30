namespace ParkEase.Booking.Events.Consumed;

/// <summary>
/// When admin deletes a driver, cancel all their active bookings.
/// This is also the compensation step in AccountDeactivationSaga.
/// </summary>
public class DriverDeletedEvent
{
    public int DriverId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }
}
