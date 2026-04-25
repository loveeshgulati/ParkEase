using MassTransit;
using ParkEase.Notification.DTOs.Requests;
using ParkEase.Notification.Events.Booking;
using ParkEase.Notification.Interfaces;

namespace ParkEase.Notification.Consumers.Booking;

public class BookingCreatedConsumer : IConsumer<BookingCreatedEvent>
{
    private readonly INotificationService _service;
    public BookingCreatedConsumer(INotificationService service) => _service = service;

    public async Task Consume(ConsumeContext<BookingCreatedEvent> context)
    {
        var e = context.Message;
        await _service.SendAsync(new SendNotificationDto
        {
            RecipientId = e.UserId,
            Title = "Booking Confirmed 🅿️",
            Message = $"Booking #{e.BookingId} confirmed for {e.VehiclePlate}. " +
                      $"Start: {e.StartTime:dd MMM HH:mm}",
            Type = "BOOKING",
            RelatedId = e.BookingId,
            RelatedType = "BOOKING",
            Channel = "APP"
        });
    }
}
