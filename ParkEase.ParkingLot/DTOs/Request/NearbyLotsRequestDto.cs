using System.ComponentModel.DataAnnotations;

namespace ParkEase.ParkingLot.DTOs.Request;

public class NearbyLotsRequestDto
{
    [Required]
    [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90.")]
    public double Latitude { get; set; }

    [Required]
    [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180.")]
    public double Longitude { get; set; }

    [Range(0.1, 100.0, ErrorMessage = "RadiusKm must be between 0.1 and 100.")]
    public double RadiusKm { get; set; } = 5.0;

    [RegularExpression("^(2W|4W|HEAVY)$", ErrorMessage = "VehicleType must be 2W, 4W, or HEAVY.")]
    public string? VehicleType { get; set; }

    public bool? EVOnly { get; set; }
}
