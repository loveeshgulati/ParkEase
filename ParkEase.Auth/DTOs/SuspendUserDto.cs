using System.ComponentModel.DataAnnotations;

namespace ParkEase.Auth.DTOs;

public class SuspendUserDto
{
    [Required] 
    public string Reason { get; set; } = string.Empty;
}
