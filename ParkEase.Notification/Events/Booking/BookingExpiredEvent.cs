namespace ParkEase.Notification.Events.Booking;

public class BookingExpiredEvent
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public int LotId { get; set; }
    public DateTime ExpiredAt { get; set; }
}
