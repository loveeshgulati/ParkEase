namespace ParkEase.Spot.Entities;

public class ParkingSpot
{
    public int SpotId { get; set; }
    public int LotId { get; set; }
    public string SpotNumber { get; set; } = string.Empty;  // e.g. A-01, B-12
    public int Floor { get; set; } = 0;                     // 0 = Ground

    // COMPACT | STANDARD | LARGE | MOTORBIKE | EV
    public string SpotType { get; set; } = string.Empty;

    // 2W | 4W | HEAVY
    public string VehicleType { get; set; } = string.Empty;

    // AVAILABLE | RESERVED | OCCUPIED
    public string Status { get; set; } = "AVAILABLE";

    public bool IsHandicapped { get; set; } = false;
    public bool IsEVCharging { get; set; } = false;
    public double PricePerHour { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public override string ToString() =>
        $"Spot[{SpotId}] {SpotNumber} Lot={LotId} Type={SpotType} Status={Status}";
}
