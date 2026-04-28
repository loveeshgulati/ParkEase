namespace ParkEase.Auth.Events.Published;

public class DriverSuspendedEvent
{
    public int DriverId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime SuspendedAt { get; set; }
}

