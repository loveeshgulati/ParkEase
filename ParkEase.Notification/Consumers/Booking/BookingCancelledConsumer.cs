using MassTransit;
using ParkEase.Notification.DTOs.Requests;
using ParkEase.Notification.Events.Booking;
using ParkEase.Notification.Interfaces;

namespace ParkEase.Notification.Consumers.Booking;

public class BookingCancelledConsumer : IConsumer<BookingCancelledEvent>
{
    private readonly INotificationService _service;
    public BookingCancelledConsumer(INotificationService service) => _service = service;

    public async Task Consume(ConsumeContext<BookingCancelledEvent> context)
    {
        var e = context.Message;
        var refundMsg = e.IsEligibleForRefund
            ? $" Refund of ₹{e.RefundAmount} will be processed."
            : string.Empty;

        await _service.SendAsync(new SendNotificationDto
        {
            RecipientId = e.UserId,
            Title = "Booking Cancelled",
            Message = $"Booking #{e.BookingId} has been cancelled.{refundMsg}",
            Type = "BOOKING",
            RelatedId = e.BookingId,
            RelatedType = "BOOKING",
            Channel = "APP"
        });
    }
}
