using System.ComponentModel.DataAnnotations;

namespace ParkEase.Vehicle.DTOs;

public class RegisterVehicleDto
{
    [Required] public string LicensePlate { get; set; } = string.Empty;
    [Required] public string Make { get; set; } = string.Empty;
    [Required] public string Model { get; set; } = string.Empty;
    [Required] public string Color { get; set; } = string.Empty;

    // 2W | 4W | HEAVY
    [Required] public string VehicleType { get; set; } = string.Empty;
    public bool IsEV { get; set; } = false;
}
