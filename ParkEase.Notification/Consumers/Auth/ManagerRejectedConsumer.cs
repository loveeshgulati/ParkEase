using MassTransit;
using ParkEase.Notification.DTOs.Requests;
using ParkEase.Auth.Events.Published;
using ParkEase.Notification.Interfaces;

namespace ParkEase.Notification.Consumers.Auth;

public class ManagerRejectedConsumer : IConsumer<ManagerRejectedEvent>
{
    private readonly INotificationService _service;
    public ManagerRejectedConsumer(INotificationService service) => _service = service;

    public async Task Consume(ConsumeContext<ManagerRejectedEvent> context)
    {
        var e = context.Message;
        await _service.SendAsync(new SendNotificationDto
        {
            RecipientId = e.ManagerId,
            Title = "Account Application Rejected",
            Message = $"Your manager application was rejected. Reason: {e.Reason}",
            Type = "REJECTION"
        });
    }
}

