namespace ParkEase.Booking.DTOs.Response;

public class BookingDto
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public int LotId { get; set; }
    public int SpotId { get; set; }
    public string VehiclePlate { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string BookingType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public double TotalAmount { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
}
