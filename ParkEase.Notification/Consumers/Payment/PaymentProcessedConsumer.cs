using MassTransit;
using ParkEase.Notification.DTOs.Requests;
using ParkEase.Notification.Events.Payment;
using ParkEase.Notification.Interfaces;

namespace ParkEase.Notification.Consumers.Payment;

public class PaymentProcessedConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly INotificationService _service;
    public PaymentProcessedConsumer(INotificationService service) => _service = service;

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var e = context.Message;
        await _service.SendAsync(new SendNotificationDto
        {
            RecipientId = e.UserId,
            Title = "Payment Successful 💰",
            Message = $"₹{e.Amount} paid via {e.Mode}. TxnId: {e.TransactionId}",
            Type = "PAYMENT",
            RelatedId = e.PaymentId,
            RelatedType = "PAYMENT",
            Channel = "APP"
        });
    }
}
