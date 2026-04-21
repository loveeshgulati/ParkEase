namespace ParkEase.Auth.Entities;

public class AuditLog
{
    public int AuditLogId { get; set; }
    public int? ActorUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? TargetUserId { get; set; }
    public string? Before { get; set; }
    public string? After { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool Success { get; set; } = true;
    public string? FailureReason { get; set; }
}
