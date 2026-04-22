namespace ParkEase.Auth.Events;

public class DriverSuspendedEvent
{
    public int DriverId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime SuspendedAt { get; set; }
}
