namespace ParkEase.Auth.DTOs;

public class PendingManagerDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
