namespace ParkEase.Notification.DTOs.Responses;

public class NotificationDto
{
    public int NotificationId { get; set; }
    public int RecipientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int? RelatedId { get; set; }
    public string? RelatedType { get; set; }
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
}

