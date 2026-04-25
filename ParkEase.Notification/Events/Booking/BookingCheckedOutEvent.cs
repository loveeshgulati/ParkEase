namespace ParkEase.Notification.Events.Booking;

public class BookingCheckedOutEvent
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public int LotId { get; set; }
    public double TotalAmount { get; set; }
    public DateTime CheckOutTime { get; set; }
}
