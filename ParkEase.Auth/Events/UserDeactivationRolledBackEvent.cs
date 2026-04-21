namespace ParkEase.Auth.Events;

public class UserDeactivationRolledBackEvent
{
    public int UserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime RolledBackAt { get; set; }
}
