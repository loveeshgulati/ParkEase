using System.ComponentModel.DataAnnotations;

namespace ParkEase.Booking.DTOs.Request;

public class CancelBookingDto
{
    [StringLength(500, ErrorMessage = "Reason must not exceed 500 characters.")]
    public string? Reason { get; set; }
}
