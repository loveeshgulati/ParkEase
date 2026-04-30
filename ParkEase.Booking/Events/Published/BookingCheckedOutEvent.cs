namespace ParkEase.Booking.Events.Published;

public class BookingCheckedOutEvent
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public int LotId { get; set; }
    public int SpotId { get; set; }
    public DateTime CheckOutTime { get; set; }
    public double TotalAmount { get; set; }
}
