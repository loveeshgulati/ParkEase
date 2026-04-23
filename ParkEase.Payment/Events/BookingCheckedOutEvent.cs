namespace ParkEase.Payment.Events;

/// <summary>
/// Event consumed from Booking-Service when booking is checked out
/// Used to auto-create payment record
/// </summary>
public class BookingCheckedOutEvent
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public int LotId { get; set; }
    public int SpotId { get; set; }
    public DateTime CheckOutTime { get; set; }
    public double TotalAmount { get; set; }
}
