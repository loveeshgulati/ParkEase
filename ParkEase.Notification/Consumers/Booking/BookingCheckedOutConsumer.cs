using MassTransit;
using ParkEase.Notification.DTOs.Requests;
using ParkEase.Notification.Events.Booking;
using ParkEase.Notification.Interfaces;

namespace ParkEase.Notification.Consumers.Booking;

public class BookingCheckedOutConsumer : IConsumer<BookingCheckedOutEvent>
{
    private readonly INotificationService _service;
    public BookingCheckedOutConsumer(INotificationService service) => _service = service;

    public async Task Consume(ConsumeContext<BookingCheckedOutEvent> context)
    {
        var e = context.Message;
        await _service.SendAsync(new SendNotificationDto
        {
            RecipientId = e.UserId,
            Title = "Checked Out 🚗",
            Message = $"Booking #{e.BookingId} completed. Total: ₹{e.TotalAmount}. Thank you!",
            Type = "CHECKOUT",
            RelatedId = e.BookingId,
            RelatedType = "BOOKING",
            Channel = "APP"
        });
    }
}
