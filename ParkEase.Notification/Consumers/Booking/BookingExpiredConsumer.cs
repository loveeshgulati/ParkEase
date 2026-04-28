using MassTransit;
using ParkEase.Notification.DTOs.Requests;
using ParkEase.Booking.Events.Published;
using ParkEase.Notification.Interfaces;

namespace ParkEase.Notification.Consumers.Booking;

public class BookingExpiredConsumer : IConsumer<BookingExpiredEvent>
{
    private readonly INotificationService _service;
    public BookingExpiredConsumer(INotificationService service) => _service = service;

    public async Task Consume(ConsumeContext<BookingExpiredEvent> context)
    {
        var e = context.Message;
        await _service.SendAsync(new SendNotificationDto
        {
            RecipientId = e.UserId,
            Title = "Booking Expired ⏰",
            Message = $"Booking #{e.BookingId} has been auto-cancelled due to no check-in within grace period.",
            Type = "EXPIRY",
            RelatedId = e.BookingId,
            RelatedType = "BOOKING"
        });
    }
}

