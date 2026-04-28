namespace ParkEase.Auth.Events.Published;

public class DriverReactivatedEvent
{
    public int DriverId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime ReactivatedAt { get; set; }
}

