using System.ComponentModel.DataAnnotations;

namespace ParkEase.Auth.DTOs;

public class RejectManagerDto
{
    [Required] 
    public string Reason { get; set; } = string.Empty;
}
