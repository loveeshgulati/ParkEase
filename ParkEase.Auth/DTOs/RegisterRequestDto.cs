using System.ComponentModel.DataAnnotations;

namespace ParkEase.Auth.DTOs;

public class RegisterRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required, MinLength(10, ErrorMessage = "Phone number must be at least 10 digits.")]
    [RegularExpression(@"^\+?[0-9\s\-]{10,15}$", ErrorMessage = "Phone number format is invalid.")]
    public string Phone { get; set; } = string.Empty;

    [RegularExpression("^(DRIVER|MANAGER)$", ErrorMessage = "Role must be DRIVER or MANAGER.")]
    public string Role { get; set; } = "DRIVER";
}
