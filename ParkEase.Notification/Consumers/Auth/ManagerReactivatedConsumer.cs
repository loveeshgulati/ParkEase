using MassTransit;
using ParkEase.Notification.DTOs.Requests;
using ParkEase.Auth.Events.Published;
using ParkEase.Notification.Interfaces;

namespace ParkEase.Notification.Consumers.Auth;

public class ManagerReactivatedConsumer : IConsumer<ManagerReactivatedEvent>
{
    private readonly INotificationService _service;
    public ManagerReactivatedConsumer(INotificationService service) => _service = service;

    public async Task Consume(ConsumeContext<ManagerReactivatedEvent> context)
    {
        var e = context.Message;
        await _service.SendAsync(new SendNotificationDto
        {
            RecipientId = e.ManagerId,
            Title = "Account Reactivated ✅",
            Message = "Your account has been reactivated. Welcome back!",
            Type = "APPROVAL"
        });
    }
}

