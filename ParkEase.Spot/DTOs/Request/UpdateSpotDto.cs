using System.ComponentModel.DataAnnotations;

namespace ParkEase.Spot.DTOs.Request;

public class UpdateSpotDto
{
    [RegularExpression("^(COMPACT|STANDARD|LARGE|MOTORBIKE|EV)$",
        ErrorMessage = "SpotType must be COMPACT, STANDARD, LARGE, MOTORBIKE, or EV.")]
    public string? SpotType { get; set; }

    [RegularExpression("^(2W|4W|HEAVY)$", ErrorMessage = "VehicleType must be 2W, 4W, or HEAVY.")]
    public string? VehicleType { get; set; }

    [Range(0.01, 10000, ErrorMessage = "PricePerHour must be between 0.01 and 10000.")]
    public double? PricePerHour { get; set; }

    public bool? IsHandicapped { get; set; }
    public bool? IsEVCharging { get; set; }
}
