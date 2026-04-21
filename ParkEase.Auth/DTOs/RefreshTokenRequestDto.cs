using System.ComponentModel.DataAnnotations;

namespace ParkEase.Auth.DTOs;

public class RefreshTokenRequestDto
{
    [Required] 
    public string RefreshToken { get; set; } = string.Empty;
}
