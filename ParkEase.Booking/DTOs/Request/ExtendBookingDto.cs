using System.ComponentModel.DataAnnotations;

namespace ParkEase.Booking.DTOs.Request;

public class ExtendBookingDto
{
    [Required(ErrorMessage = "NewEndTime is required.")]
    public DateTime NewEndTime { get; set; }
}
