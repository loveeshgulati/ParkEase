using System.ComponentModel.DataAnnotations;

namespace ParkEase.Auth.DTOs;

public class UpdateProfileDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters.")]
    public string? FullName { get; set; }

    [RegularExpression(@"^\+?[0-9\s\-]{10,15}$", ErrorMessage = "Phone number format is invalid.")]
    public string? Phone { get; set; }

    [Url(ErrorMessage = "Profile picture URL must be a valid URL.")]
    public string? ProfilePicUrl { get; set; }

    [RegularExpression(@"^[A-Z0-9\-]{2,15}$", ErrorMessage = "Vehicle plate format is invalid.")]
    public string? VehiclePlate { get; set; }
}
