using MassTransit;
using ParkEase.Notification.DTOs.Requests;
using ParkEase.Notification.Events.Auth;
using ParkEase.Notification.Interfaces;

namespace ParkEase.Notification.Consumers.Auth;

public class DriverSuspendedConsumer : IConsumer<DriverSuspendedEvent>
{
    private readonly INotificationService _service;
    public DriverSuspendedConsumer(INotificationService service) => _service = service;

    public async Task Consume(ConsumeContext<DriverSuspendedEvent> context)
    {
        var e = context.Message;
        await _service.SendAsync(new SendNotificationDto
        {
            RecipientId = e.DriverId,
            Title = "Account Suspended",
            Message = $"Your account has been suspended. Reason: {e.Reason}. Contact support.",
            Type = "SUSPENSION",
            Channel = "APP"
        });
    }
}
