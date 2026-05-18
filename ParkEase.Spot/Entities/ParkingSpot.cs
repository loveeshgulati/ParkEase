namespace ParkEase.Spot.Entities;

public class ParkingSpot
{
    public int SpotId { get; set; }
    public int LotId { get; set; }
    public string SpotNumber { get; set; } = string.Empty;  
    public int Floor { get; set; } = 0;                     

    
    public string SpotType { get; set; } = string.Empty;

    
    public string VehicleType { get; set; } = string.Empty;

    
    public string Status { get; set; } = "AVAILABLE";

    public bool IsHandicapped { get; set; } = false;
    public bool IsEVCharging { get; set; } = false;
    public double PricePerHour { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public override string ToString() =>
        $"Spot[{SpotId}] {SpotNumber} Lot={LotId} Type={SpotType} Status={Status}";
}
