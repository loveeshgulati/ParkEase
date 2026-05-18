namespace ParkEase.Vehicle.Events;







public class DriverDeletedEvent
{
    public int DriverId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }
}
