namespace ParkEase.Spot.DTOs.Response;

public class SpotDto
{
    public int SpotId { get; set; }
    public int LotId { get; set; }
    public string SpotNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public string SpotType { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsHandicapped { get; set; }
    public bool IsEVCharging { get; set; }
    public double PricePerHour { get; set; }
    public DateTime CreatedAt { get; set; }
}
