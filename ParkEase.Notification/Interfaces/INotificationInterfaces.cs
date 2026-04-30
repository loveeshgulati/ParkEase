using ParkEase.Notification.DTOs.Requests;
using ParkEase.Notification.DTOs.Responses;
using ParkEase.Notification.Entities;

namespace ParkEase.Notification.Interfaces;

public interface INotificationRepository
{
    Task<ParkEase.Notification.Entities.Notification?> FindByNotificationIdAsync(int notificationId);
    Task<List<ParkEase.Notification.Entities.Notification>> FindByRecipientIdAsync(int recipientId);
    Task<List<ParkEase.Notification.Entities.Notification>> FindUnreadByRecipientIdAsync(int recipientId);
    Task<int> CountUnreadByRecipientIdAsync(int recipientId);
    Task<List<ParkEase.Notification.Entities.Notification>> GetAllAsync();
    Task<ParkEase.Notification.Entities.Notification> CreateAsync(ParkEase.Notification.Entities.Notification notification);
    Task<ParkEase.Notification.Entities.Notification> UpdateAsync(ParkEase.Notification.Entities.Notification notification);
    Task MarkAllReadByRecipientAsync(int recipientId);
    Task DeleteByNotificationIdAsync(int notificationId);
}

public interface INotificationService
{
    // ── Send ─────────────────────────────────────────────────────────────────
    Task<NotificationDto> SendAsync(SendNotificationDto request);
    Task SendBulkAsync(List<SendNotificationDto> requests);
    Task BroadcastAsync(BroadcastNotificationDto request, List<int> recipientIds);

    // ── Read ─────────────────────────────────────────────────────────────────
    Task<List<NotificationDto>> GetByRecipientAsync(int recipientId);
    Task<List<NotificationDto>> GetUnreadAsync(int recipientId);
    Task<int> GetUnreadCountAsync(int recipientId);

    // ── Manage ───────────────────────────────────────────────────────────────
    Task MarkAsReadAsync(int notificationId, int recipientId);
    Task MarkAllReadAsync(int recipientId);
    Task DeleteAsync(int notificationId, int recipientId);

    // ── Admin ────────────────────────────────────────────────────────────────
    Task<List<NotificationDto>> GetAllAsync();
}
