using System.ComponentModel.DataAnnotations;

namespace ParkEase.Booking.DTOs.Request;

public class CreateBookingDto
{
    [Required] public int LotId { get; set; }
    [Required] public int SpotId { get; set; }

    [Required]
    [RegularExpression(@"^[A-Z0-9\-]{2,15}$", ErrorMessage = "Vehicle plate format is invalid.")]
    public string VehiclePlate { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(2W|4W|HEAVY)$", ErrorMessage = "VehicleType must be 2W, 4W, or HEAVY.")]
    public string VehicleType { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(PRE_BOOKING|WALK_IN)$", ErrorMessage = "BookingType must be PRE_BOOKING or WALK_IN.")]
    public string BookingType { get; set; } = string.Empty;

    [Required] public DateTime StartTime { get; set; }
    [Required] public DateTime EndTime { get; set; }
}
