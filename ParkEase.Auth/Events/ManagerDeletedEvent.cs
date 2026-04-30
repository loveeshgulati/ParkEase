namespace ParkEase.Auth.Events;

public class ManagerDeletedEvent
{
    public int ManagerId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }
}
