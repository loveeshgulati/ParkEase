namespace ParkEase.Spot.DTOs.Request;

public class UpdateSpotDto
{
    public string? SpotType { get; set; }
    public string? VehicleType { get; set; }
    public double? PricePerHour { get; set; }
    public bool? IsHandicapped { get; set; }
    public bool? IsEVCharging { get; set; }
}
