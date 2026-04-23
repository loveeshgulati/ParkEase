namespace ParkEase.Booking.DTOs.Response;

public class FareCalculationDto
{
    public int BookingId { get; set; }
    public int SpotId { get; set; }
    public double PricePerHour { get; set; }
    public double HoursParked { get; set; }
    public double TotalAmount { get; set; }
    public string Note { get; set; } = string.Empty;
}
