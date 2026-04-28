namespace ParkEase.Auth.Events.Published;

public class ManagerApprovedEvent
{
    public int ManagerId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int ApprovedByAdminId { get; set; }
    public DateTime ApprovedAt { get; set; }
}

