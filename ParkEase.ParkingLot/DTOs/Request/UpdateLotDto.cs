using System.ComponentModel.DataAnnotations;

namespace ParkEase.ParkingLot.DTOs.Request;

public class UpdateLotDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string? Name { get; set; }

    [StringLength(250, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 250 characters.")]
    public string? Address { get; set; }

    [StringLength(100, MinimumLength = 2, ErrorMessage = "City must be between 2 and 100 characters.")]
    public string? City { get; set; }

    [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90.")]
    public double? Latitude { get; set; }

    [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180.")]
    public double? Longitude { get; set; }

    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "OpenTime must be in HH:mm format (e.g. 08:00).")]
    public string? OpenTime { get; set; }

    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "CloseTime must be in HH:mm format (e.g. 22:00).")]
    public string? CloseTime { get; set; }
}
