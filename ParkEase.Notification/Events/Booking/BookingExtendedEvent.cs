namespace ParkEase.Notification.Events.Booking;

public class BookingExtendedEvent
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public DateTime NewEndTime { get; set; }
    public DateTime ExtendedAt { get; set; }
}
