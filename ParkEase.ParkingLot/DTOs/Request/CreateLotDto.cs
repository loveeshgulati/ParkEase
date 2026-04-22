using System.ComponentModel.DataAnnotations;

namespace ParkEase.ParkingLot.DTOs.Request;

public class CreateLotDto
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Address { get; set; } = string.Empty;
    [Required] public string City { get; set; } = string.Empty;
    [Required] public double Latitude { get; set; }
    [Required] public double Longitude { get; set; }
    [Required] public string OpenTime { get; set; } = string.Empty;   // "08:00"
    [Required] public string CloseTime { get; set; } = string.Empty;  // "22:00"
}
