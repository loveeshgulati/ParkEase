using System.ComponentModel.DataAnnotations;

namespace ParkEase.Spot.DTOs.Request;

public class BulkAddSpotDto
{
    [Required] public int LotId { get; set; }

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

    [Required]
    [Range(1, 500, ErrorMessage = "Count must be between 1 and 500.")]
    public int Count { get; set; }

    [Required]
    [RegularExpression("^[A-Z0-9]{1,5}$", ErrorMessage = "Prefix must be 1-5 uppercase alphanumeric characters.")]
    public string Prefix { get; set; } = string.Empty;
}
