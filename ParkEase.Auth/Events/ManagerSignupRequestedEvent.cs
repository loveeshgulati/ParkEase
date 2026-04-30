namespace ParkEase.Auth.Events;

public class ManagerSignupRequestedEvent
{
    public int ManagerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
}
