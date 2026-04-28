using System.ComponentModel.DataAnnotations;

namespace ParkEase.Notification.DTOs.Requests;

public class BroadcastNotificationDto
{
    [Required] public string Title { get; set; } = string.Empty;
    [Required] public string Message { get; set; } = string.Empty;

    // ALL | DRIVER | MANAGER
    [Required] public string TargetRole { get; set; } = "ALL";
}
