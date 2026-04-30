using Microsoft.EntityFrameworkCore;
using ParkEase.Notification.Data;
using ParkEase.Notification.Entities;
using ParkEase.Notification.Interfaces;

namespace ParkEase.Notification.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context) => _context = context;

    public async Task<ParkEase.Notification.Entities.Notification?> FindByNotificationIdAsync(int notificationId) =>
        await _context.Notifications.FindAsync(notificationId);

    public async Task<List<ParkEase.Notification.Entities.Notification>> FindByRecipientIdAsync(int recipientId) =>
        await _context.Notifications
            .Where(n => n.RecipientId == recipientId)
            .OrderByDescending(n => n.SentAt)
            .ToListAsync();

    public async Task<List<ParkEase.Notification.Entities.Notification>> FindUnreadByRecipientIdAsync(int recipientId) =>
        await _context.Notifications
            .Where(n => n.RecipientId == recipientId && !n.IsRead)
            .OrderByDescending(n => n.SentAt)
            .ToListAsync();

    public async Task<int> CountUnreadByRecipientIdAsync(int recipientId) =>
        await _context.Notifications
            .CountAsync(n => n.RecipientId == recipientId && !n.IsRead);

    public async Task<List<ParkEase.Notification.Entities.Notification>> GetAllAsync() =>
        await _context.Notifications
            .OrderByDescending(n => n.SentAt)
            .ToListAsync();

    public async Task<ParkEase.Notification.Entities.Notification> CreateAsync(ParkEase.Notification.Entities.Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<ParkEase.Notification.Entities.Notification> UpdateAsync(ParkEase.Notification.Entities.Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task MarkAllReadByRecipientAsync(int recipientId)
    {
        var unread = await _context.Notifications
            .Where(n => n.RecipientId == recipientId && !n.IsRead)
            .ToListAsync();

        unread.ForEach(n => n.IsRead = true);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteByNotificationIdAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }
}
