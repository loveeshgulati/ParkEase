using System.ComponentModel.DataAnnotations;

namespace ParkEase.Vehicle.DTOs;

public class UpdateVehicleDto
{
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Make must be between 1 and 50 characters.")]
    public string? Make { get; set; }

    [StringLength(50, MinimumLength = 1, ErrorMessage = "Model must be between 1 and 50 characters.")]
    public string? Model { get; set; }

    [StringLength(30, MinimumLength = 1, ErrorMessage = "Color must be between 1 and 30 characters.")]
    public string? Color { get; set; }

    [RegularExpression("^(2W|4W|HEAVY)$", ErrorMessage = "VehicleType must be 2W, 4W, or HEAVY.")]
    public string? VehicleType { get; set; }

    public bool? IsEV { get; set; }
}
