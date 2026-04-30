namespace ParkEase.Auth.Events;

public class UserDeactivatedEvent
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime DeactivatedAt { get; set; }
}
