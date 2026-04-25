namespace ParkEase.Notification.Entities;

public class Notification
{
    public int NotificationId { get; set; }
    public int RecipientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // BOOKING | CHECKIN | EXPIRY | CHECKOUT | PAYMENT
    // REFUND | APPROVAL | REJECTION | SUSPENSION | WELCOME | PROMO
    public string Type { get; set; } = string.Empty;

    // APP | EMAIL | SMS
    public string Channel { get; set; } = "APP";

    public int? RelatedId { get; set; }
    public string? RelatedType { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public override string ToString() =>
        $"Notification[{NotificationId}] To={RecipientId} Type={Type} Read={IsRead}";
}
