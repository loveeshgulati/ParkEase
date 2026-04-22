namespace ParkEase.Spot.Events.Published;

public class SpotAddedEvent
{
    public int SpotId { get; set; }
    public int LotId { get; set; }
    public string SpotNumber { get; set; } = string.Empty;
    public string SpotType { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public double PricePerHour { get; set; }
    public bool IsEVCharging { get; set; }
    public bool IsHandicapped { get; set; }
}
