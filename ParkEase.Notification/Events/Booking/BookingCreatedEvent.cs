namespace ParkEase.Notification.Events.Booking;

public class BookingCreatedEvent
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public int LotId { get; set; }
    public int SpotId { get; set; }
    public string VehiclePlate { get; set; } = string.Empty;
    public string BookingType { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
}
