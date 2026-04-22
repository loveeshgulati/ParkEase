using System.ComponentModel.DataAnnotations;

namespace ParkEase.Spot.DTOs.Request;

public class AddSpotDto
{
    [Required] public int LotId { get; set; }
    [Required] public string SpotNumber { get; set; } = string.Empty;
    public int Floor { get; set; } = 0;

    // COMPACT | STANDARD | LARGE | MOTORBIKE | EV
    [Required] public string SpotType { get; set; } = string.Empty;

    // 2W | 4W | HEAVY
    [Required] public string VehicleType { get; set; } = string.Empty;

    [Required] public double PricePerHour { get; set; }
    public bool IsHandicapped { get; set; } = false;
    public bool IsEVCharging { get; set; } = false;
}
