namespace ParkEase.Booking.Entities;

public class Booking
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public int LotId { get; set; }
    public int SpotId { get; set; }
    public string VehiclePlate { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;

    // PRE_BOOKING | WALK_IN
    public string BookingType { get; set; } = string.Empty;

    // RESERVED | ACTIVE | COMPLETED | CANCELLED | EXPIRED
    public string Status { get; set; } = "RESERVED";

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public double TotalAmount { get; set; } = 0;
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public override string ToString() =>
        $"Booking[{BookingId}] User={UserId} Spot={SpotId} Status={Status}";
}
