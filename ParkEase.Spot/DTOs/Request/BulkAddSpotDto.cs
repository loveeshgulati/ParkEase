using System.ComponentModel.DataAnnotations;

namespace ParkEase.Spot.DTOs.Request;

public class BulkAddSpotDto
{
    [Required] public int LotId { get; set; }
    [Required] public int Floor { get; set; } = 0;

    // COMPACT | STANDARD | LARGE | MOTORBIKE | EV
    [Required] public string SpotType { get; set; } = string.Empty;

    // 2W | 4W | HEAVY
    [Required] public string VehicleType { get; set; } = string.Empty;

    [Required] public double PricePerHour { get; set; }
    public bool IsHandicapped { get; set; } = false;
    public bool IsEVCharging { get; set; } = false;

    // How many spots to create
    [Required] public int Count { get; set; }

    // Prefix for spot numbers e.g. "A" → A-01, A-02...
    [Required] public string Prefix { get; set; } = string.Empty;
}
