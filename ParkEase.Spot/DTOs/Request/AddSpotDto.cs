using System.ComponentModel.DataAnnotations;

namespace ParkEase.Spot.DTOs.Request;

public class AddSpotDto
{
    [Required] public int LotId { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "SpotNumber must be between 1 and 20 characters.")]
    public string SpotNumber { get; set; } = string.Empty;

    public int Floor { get; set; } = 0;

    [Required]
    [RegularExpression("^(COMPACT|STANDARD|LARGE|MOTORBIKE|EV)$",
        ErrorMessage = "SpotType must be COMPACT, STANDARD, LARGE, MOTORBIKE, or EV.")]
    public string SpotType { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(2W|4W|HEAVY)$", ErrorMessage = "VehicleType must be 2W, 4W, or HEAVY.")]
    public string VehicleType { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 10000, ErrorMessage = "PricePerHour must be between 0.01 and 10000.")]
    public double PricePerHour { get; set; }

    public bool IsHandicapped { get; set; } = false;
    public bool IsEVCharging { get; set; } = false;
}
