using MassTransit;
using ParkEase.Notification.DTOs.Requests;
using ParkEase.Booking.Events.Published;
using ParkEase.Notification.Interfaces;

namespace ParkEase.Notification.Consumers.Booking;

public class BookingCheckedInConsumer : IConsumer<BookingCheckedInEvent>
{
    private readonly INotificationService _service;
    public BookingCheckedInConsumer(INotificationService service) => _service = service;

    public async Task Consume(ConsumeContext<BookingCheckedInEvent> context)
    {
        var e = context.Message;
        await _service.SendAsync(new SendNotificationDto
        {
            RecipientId = e.UserId,
            Title = "Checked In ✅",
            Message = $"You have checked in to Spot #{e.SpotId}. Have a great parking experience!",
            Type = "CHECKIN",
            RelatedId = e.BookingId,
            RelatedType = "BOOKING"
        });
    }
}

