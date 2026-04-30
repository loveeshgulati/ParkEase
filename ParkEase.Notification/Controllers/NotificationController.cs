using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkEase.Notification.DTOs.Requests;
using ParkEase.Notification.DTOs.Responses;
using ParkEase.Notification.DTOs.Common;
using ParkEase.Notification.Interfaces;

namespace ParkEase.Notification.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
[Produces("application/json")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        INotificationService service,
        ILogger<NotificationController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // GET /api/v1/notifications
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var result = await _service.GetByRecipientAsync(GetCurrentUserId());
        return Ok(ApiResponse<List<NotificationDto>>.Ok(result,
            $"{result.Count} notifications"));
    }

    // GET /api/v1/notifications/unread
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread()
    {
        var result = await _service.GetUnreadAsync(GetCurrentUserId());
        return Ok(ApiResponse<List<NotificationDto>>.Ok(result,
            $"{result.Count} unread"));
    }

    // GET /api/v1/notifications/unread-count
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var count = await _service.GetUnreadCountAsync(GetCurrentUserId());
        return Ok(ApiResponse<int>.Ok(count));
    }

    // PUT /api/v1/notifications/{id}/read
    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        try
        {
            await _service.MarkAsReadAsync(id, GetCurrentUserId());
            return Ok(ApiResponse<object>.Ok(null!, "Marked as read"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    // PUT /api/v1/notifications/read-all
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        await _service.MarkAllReadAsync(GetCurrentUserId());
        return Ok(ApiResponse<object>.Ok(null!, "All notifications marked as read"));
    }

    // DELETE /api/v1/notifications/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id, GetCurrentUserId());
            return Ok(ApiResponse<object>.Ok(null!, "Notification deleted"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
    // POST /api/v1/notifications/send
[HttpPost("send")]
[Authorize(Roles = "ADMIN")]
public async Task<IActionResult> Send([FromBody] SendNotificationDto request)
{
    var result = await _service.SendAsync(request);
    return Ok(ApiResponse<NotificationDto>.Ok(result, "Notification sent"));
}
    // POST /api/v1/notifications/broadcast  (Admin only)
    [HttpPost("broadcast")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Broadcast(
        [FromBody] BroadcastNotificationDto request,
        [FromQuery] List<int> recipientIds)
    {
        await _service.BroadcastAsync(request, recipientIds);
        return Ok(ApiResponse<object>.Ok(null!,
            $"Broadcast sent to {recipientIds.Count} users"));
    }

    // GET /api/v1/notifications/all  (Admin only)
    [HttpGet("all")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResponse<List<NotificationDto>>.Ok(result,
            $"{result.Count} total notifications"));
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("userId")?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token.");
        return int.Parse(claim);
    }
}
