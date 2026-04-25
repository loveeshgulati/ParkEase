using MassTransit;
using ParkEase.Notification.DTOs.Requests;
using ParkEase.Notification.Events.Auth;
using ParkEase.Notification.Interfaces;

namespace ParkEase.Notification.Consumers.Auth;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly INotificationService _service;
    public UserRegisteredConsumer(INotificationService service) => _service = service;

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var e = context.Message;
        await _service.SendAsync(new SendNotificationDto
        {
            RecipientId = e.UserId,
            Title = "Welcome to ParkEase! 🎉",
            Message = $"Hi {e.FullName}, your account has been created successfully.",
            Type = "WELCOME",
            Channel = "APP"
        });
    }
}
