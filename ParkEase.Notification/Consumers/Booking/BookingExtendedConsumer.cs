using MassTransit;
using ParkEase.Notification.DTOs.Requests;
using ParkEase.Booking.Events.Published;
using ParkEase.Notification.Interfaces;

namespace ParkEase.Notification.Consumers.Booking;

public class BookingExtendedConsumer : IConsumer<BookingExtendedEvent>
{
    private readonly INotificationService _service;
    public BookingExtendedConsumer(INotificationService service) => _service = service;

    public async Task Consume(ConsumeContext<BookingExtendedEvent> context)
    {
        var e = context.Message;
        await _service.SendAsync(new SendNotificationDto
        {
            RecipientId = e.UserId,
            Title = "Booking Extended ⏱️",
            Message = $"Booking #{e.BookingId} extended to {e.NewEndTime:dd MMM HH:mm}.",
            Type = "BOOKING",
            RelatedId = e.BookingId,
            RelatedType = "BOOKING"
        });
    }
}

