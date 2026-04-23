using MassTransit;
using ParkEase.Payment.Data;
using ParkEase.Payment.Events;

namespace ParkEase.Payment.Consumers;

/// <summary>
/// When booking is cancelled and eligible for refund,
/// auto-process the refund.
/// </summary>
public class BookingCancelledConsumer : IConsumer<BookingCancelledEvent>
{
    private readonly PaymentDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<BookingCancelledConsumer> _logger;

    public BookingCancelledConsumer(
        PaymentDbContext context,
        IPublishEndpoint publishEndpoint,
        ILogger<BookingCancelledConsumer> logger)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BookingCancelledEvent> context)
    {
        var evt = context.Message;

        if (!evt.IsEligibleForRefund || evt.RefundAmount <= 0) return;

        var payment = _context.Payments
            .FirstOrDefault(p => p.BookingId == evt.BookingId
                && p.Status == "PAID");

        if (payment == null) return;

        payment.Status = "REFUNDED";
        payment.RefundAmount = evt.RefundAmount;
        payment.RefundReason = evt.Reason;
        payment.RefundedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _publishEndpoint.Publish(new RefundProcessedEvent
        {
            PaymentId = payment.PaymentId,
            BookingId = evt.BookingId,
            UserId = evt.UserId,
            RefundAmount = evt.RefundAmount,
            Reason = evt.Reason,
            RefundedAt = payment.RefundedAt!.Value
        });

        _logger.LogInformation(
            "Refund ₹{Amount} processed for Booking {BookingId}",
            evt.RefundAmount, evt.BookingId);
    }
}
