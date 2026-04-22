using System.ComponentModel.DataAnnotations;

namespace ParkEase.ParkingLot.DTOs.Request;

public class RejectLotDto
{
    [Required] public string Reason { get; set; } = string.Empty;
}
