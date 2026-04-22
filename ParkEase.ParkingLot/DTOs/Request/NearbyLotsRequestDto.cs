using System.ComponentModel.DataAnnotations;

namespace ParkEase.ParkingLot.DTOs.Request;

public class NearbyLotsRequestDto
{
    [Required] public double Latitude { get; set; }
    [Required] public double Longitude { get; set; }
    public double RadiusKm { get; set; } = 5.0;
    public string? VehicleType { get; set; }
    public bool? EVOnly { get; set; }
}
