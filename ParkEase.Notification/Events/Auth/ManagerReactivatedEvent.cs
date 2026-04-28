namespace ParkEase.Auth.Events.Published;

public class ManagerReactivatedEvent
{
    public int ManagerId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime ReactivatedAt { get; set; }
}

