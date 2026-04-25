using MassTransit;
using ParkEase.Notification.DTOs.Requests;
using ParkEase.Notification.Events.Payment;
using ParkEase.Notification.Interfaces;

namespace ParkEase.Notification.Consumers.Payment;

public class RefundProcessedConsumer : IConsumer<RefundProcessedEvent>
{
    private readonly INotificationService _service;
    public RefundProcessedConsumer(INotificationService service) => _service = service;

    public async Task Consume(ConsumeContext<RefundProcessedEvent> context)
    {
        var e = context.Message;
        await _service.SendAsync(new SendNotificationDto
        {
            RecipientId = e.UserId,
            Title = "Refund Processed 💸",
            Message = $"₹{e.RefundAmount} refund processed for Booking #{e.BookingId}.",
            Type = "REFUND",
            RelatedId = e.PaymentId,
            RelatedType = "PAYMENT",
            Channel = "APP"
        });
    }
}
