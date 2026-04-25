namespace ParkEase.Notification.Events.Booking;

public class BookingCheckedInEvent
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public int LotId { get; set; }
    public int SpotId { get; set; }
    public DateTime CheckInTime { get; set; }
}
