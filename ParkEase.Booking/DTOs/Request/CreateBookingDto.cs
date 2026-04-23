using System.ComponentModel.DataAnnotations;

namespace ParkEase.Booking.DTOs.Request;

public class CreateBookingDto
{
    [Required] public int LotId { get; set; }
    [Required] public int SpotId { get; set; }
    [Required] public string VehiclePlate { get; set; } = string.Empty;
    [Required] public string VehicleType { get; set; } = string.Empty;

    // PRE_BOOKING | WALK_IN
    [Required] public string BookingType { get; set; } = string.Empty;

    [Required] public DateTime StartTime { get; set; }
    [Required] public DateTime EndTime { get; set; }
}
