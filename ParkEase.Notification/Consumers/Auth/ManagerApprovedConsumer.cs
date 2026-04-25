using MassTransit;
using ParkEase.Notification.DTOs.Requests;
using ParkEase.Notification.Events.Auth;
using ParkEase.Notification.Interfaces;

namespace ParkEase.Notification.Consumers.Auth;

public class ManagerApprovedConsumer : IConsumer<ManagerApprovedEvent>
{
    private readonly INotificationService _service;
    public ManagerApprovedConsumer(INotificationService service) => _service = service;

    public async Task Consume(ConsumeContext<ManagerApprovedEvent> context)
    {
        var e = context.Message;
        await _service.SendAsync(new SendNotificationDto
        {
            RecipientId = e.ManagerId,
            Title = "Account Approved ✅",
            Message = "Your manager account has been approved. You can now create parking lots.",
            Type = "APPROVAL",
            Channel = "APP"
        });
    }
}
