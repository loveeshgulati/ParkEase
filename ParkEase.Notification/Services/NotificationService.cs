using Microsoft.AspNetCore.SignalR;
using ParkEase.Notification.DTOs.Requests;
using ParkEase.Notification.DTOs.Responses;
using ParkEase.Notification.DTOs.Common;
using ParkEase.Notification.Entities;
using ParkEase.Notification.Hubs;
using ParkEase.Notification.Interfaces;

namespace ParkEase.Notification.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository repository,
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _repository = repository;
        _hubContext = hubContext;
        _logger = logger;
    }

    // ── Send single notification ──────────────────────────────────────────────
    public async Task<NotificationDto> SendAsync(SendNotificationDto request)
    {
        var notification = new ParkEase.Notification.Entities.Notification
        {
            RecipientId = request.RecipientId,
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            RelatedId = request.RelatedId,
            RelatedType = request.RelatedType,
            IsRead = false,
            SentAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(notification);

        // Push real-time via SignalR to connected client
        await _hubContext.Clients
            .Group($"user_{request.RecipientId}")
            .SendAsync("ReceiveNotification", new
            {
                created.NotificationId,
                created.Title,
                created.Message,
                created.Type,
                created.SentAt
            });

        _logger.LogInformation(
            "Notification sent to User {UserId}: [{Type}] {Title}",
            request.RecipientId, request.Type, request.Title);

        return MapToDto(created);
    }

    // ── Send bulk notifications ───────────────────────────────────────────────
    public async Task SendBulkAsync(List<SendNotificationDto> requests)
    {
        foreach (var request in requests)
            await SendAsync(request);
    }

    // ── Broadcast to all users of a role ─────────────────────────────────────
    public async Task BroadcastAsync(
        BroadcastNotificationDto request, List<int> recipientIds)
    {
        var tasks = recipientIds.Select(id => SendAsync(new SendNotificationDto
        {
            RecipientId = id,
            Title = request.Title,
            Message = request.Message,
            Type = "PROMO"
        }));

        await Task.WhenAll(tasks);

        _logger.LogInformation(
            "Broadcast sent to {Count} users: {Title}",
            recipientIds.Count, request.Title);
    }

    // ── Get all notifications for a user ──────────────────────────────────────
    public async Task<List<NotificationDto>> GetByRecipientAsync(int recipientId)
    {
        var notifications = await _repository.FindByRecipientIdAsync(recipientId);
        return notifications.Select(MapToDto).ToList();
    }

    // ── Get unread notifications ──────────────────────────────────────────────
    public async Task<List<NotificationDto>> GetUnreadAsync(int recipientId)
    {
        var notifications = await _repository.FindUnreadByRecipientIdAsync(recipientId);
        return notifications.Select(MapToDto).ToList();
    }

    // ── Get unread count ──────────────────────────────────────────────────────
    public async Task<int> GetUnreadCountAsync(int recipientId) =>
        await _repository.CountUnreadByRecipientIdAsync(recipientId);

    // ── Mark single notification as read ──────────────────────────────────────
    public async Task MarkAsReadAsync(int notificationId, int recipientId)
    {
        var notification = await _repository.FindByNotificationIdAsync(notificationId)
            ?? throw new KeyNotFoundException(
                $"Notification {notificationId} not found.");

        if (notification.RecipientId != recipientId)
            throw new UnauthorizedAccessException(
                "You can only mark your own notifications as read.");

        notification.IsRead = true;
        await _repository.UpdateAsync(notification);
    }

    // ── Mark all as read ──────────────────────────────────────────────────────
    public async Task MarkAllReadAsync(int recipientId) =>
        await _repository.MarkAllReadByRecipientAsync(recipientId);

    // ── Delete notification ───────────────────────────────────────────────────
    public async Task DeleteAsync(int notificationId, int recipientId)
    {
        var notification = await _repository.FindByNotificationIdAsync(notificationId)
            ?? throw new KeyNotFoundException(
                $"Notification {notificationId} not found.");

        if (notification.RecipientId != recipientId)
            throw new UnauthorizedAccessException(
                "You can only delete your own notifications.");

        await _repository.DeleteByNotificationIdAsync(notificationId);
    }

    // ── Get all (Admin) ───────────────────────────────────────────────────────
    public async Task<List<NotificationDto>> GetAllAsync()
    {
        var notifications = await _repository.GetAllAsync();
        return notifications.Select(MapToDto).ToList();
    }

    // ── Mapper ────────────────────────────────────────────────────────────────
    public static NotificationDto MapToDto(ParkEase.Notification.Entities.Notification n) => new()
    {
        NotificationId = n.NotificationId,
        RecipientId = n.RecipientId,
        Title = n.Title,
        Message = n.Message,
        Type = n.Type,
        RelatedId = n.RelatedId,
        RelatedType = n.RelatedType,
        IsRead = n.IsRead,
        SentAt = n.SentAt
    };
}
