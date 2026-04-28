using MassTransit;
using ParkEase.Notification.DTOs.Requests;
using ParkEase.Auth.Events.Published;
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
            Type = "WELCOME"
        });

        // If it's a manager, notify the admin
        if (e.Role == "MANAGER")
        {
            await _service.SendAsync(new SendNotificationDto
            {
                RecipientId = 1, // Platform Admin
                Title = "New Manager Registration 📋",
                Message = $"{e.FullName} ({e.Email}) has registered as a Manager and is awaiting your approval.",
                Type = "SYSTEM",
                RelatedId = e.UserId,
                RelatedType = "MANAGER"
            });
        }
    }
}

