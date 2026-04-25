using System.ComponentModel.DataAnnotations;

namespace ParkEase.Notification.DTOs.Requests;

public class SendNotificationDto
{
    [Required] public int RecipientId { get; set; }
    [Required] public string Title { get; set; } = string.Empty;
    [Required] public string Message { get; set; } = string.Empty;
    [Required] public string Type { get; set; } = string.Empty;
    public string Channel { get; set; } = "APP";
    public int? RelatedId { get; set; }
    public string? RelatedType { get; set; }
}
