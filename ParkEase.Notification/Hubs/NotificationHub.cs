using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ParkEase.Notification.Hubs;

/// <summary>
/// SignalR Hub for real-time in-app notifications.
/// Connected clients join a group named by their userId.
/// When a notification is sent to a user, it's pushed via their group.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
{
    var userId = Context.User?.FindFirst("userId")?.Value
               ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? Context.User?.FindFirst("sub")?.Value;

    if (!string.IsNullOrEmpty(userId))
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        _logger.LogInformation("User {UserId} connected to NotificationHub", userId);
    }
    else
    {
        _logger.LogWarning("SignalR connection with no userId claim. ConnectionId: {Id}", Context.ConnectionId);
    }

    await base.OnConnectedAsync();
}

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("userId")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation(
                "User {UserId} disconnected from NotificationHub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
