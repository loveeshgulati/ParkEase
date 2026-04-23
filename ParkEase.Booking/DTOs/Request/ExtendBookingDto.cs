using System.ComponentModel.DataAnnotations;

namespace ParkEase.Booking.DTOs.Request;

public class ExtendBookingDto
{
    [Required] public DateTime NewEndTime { get; set; }
}
