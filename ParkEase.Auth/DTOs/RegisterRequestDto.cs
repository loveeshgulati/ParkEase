using System.ComponentModel.DataAnnotations;

namespace ParkEase.Auth.DTOs;

public class RegisterRequestDto
{
    [Required] 
    public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress] 
    public string Email { get; set; } = string.Empty;
    [Required, MinLength(6)] 
    public string Password { get; set; } = string.Empty;
    [Required, MinLength(10)] 
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = "DRIVER"; // DRIVER | MANAGER
}
