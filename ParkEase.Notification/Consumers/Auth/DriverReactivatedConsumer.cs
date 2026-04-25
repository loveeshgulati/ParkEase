using MassTransit;
using ParkEase.Notification.DTOs.Requests;
using ParkEase.Notification.Events.Auth;
using ParkEase.Notification.Interfaces;

namespace ParkEase.Notification.Consumers.Auth;

public class DriverReactivatedConsumer : IConsumer<DriverReactivatedEvent>
{
    private readonly INotificationService _service;
    public DriverReactivatedConsumer(INotificationService service) => _service = service;

    public async Task Consume(ConsumeContext<DriverReactivatedEvent> context)
    {
        var e = context.Message;
        await _service.SendAsync(new SendNotificationDto
        {
            RecipientId = e.DriverId,
            Title = "Account Reactivated ✅",
            Message = "Your account has been reactivated. Welcome back!",
            Type = "APPROVAL",
            Channel = "APP"
        });
    }
}
