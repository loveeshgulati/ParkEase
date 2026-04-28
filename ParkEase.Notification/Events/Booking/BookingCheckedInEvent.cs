namespace ParkEase.Booking.Events.Published;

public class BookingCheckedInEvent
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public int LotId { get; set; }
    public int SpotId { get; set; }
    public DateTime CheckInTime { get; set; }
}

