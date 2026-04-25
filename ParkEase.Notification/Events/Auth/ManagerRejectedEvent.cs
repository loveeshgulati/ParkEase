namespace ParkEase.Notification.Events.Auth;

public class ManagerRejectedEvent
{
    public int ManagerId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime RejectedAt { get; set; }
}
