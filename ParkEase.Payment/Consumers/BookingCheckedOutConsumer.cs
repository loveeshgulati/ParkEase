using MassTransit;
using ParkEase.Payment.Data;
using PaymentEntity = ParkEase.Payment.Entities.Payment;
using ParkEase.Payment.Events;

namespace ParkEase.Payment.Consumers;

/// <summary>
/// When booking-service completes a checkout,
/// auto-create a PENDING payment record.
/// Driver then calls /payments/process to complete payment.
/// </summary>
public class BookingCheckedOutConsumer : IConsumer<BookingCheckedOutEvent>
{
    private readonly PaymentDbContext _context;
    private readonly ILogger<BookingCheckedOutConsumer> _logger;

    public BookingCheckedOutConsumer(
        PaymentDbContext context,
        ILogger<BookingCheckedOutConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BookingCheckedOutEvent> context)
    {
        var evt = context.Message;

        // Check if payment already exists for this booking
        var existing = _context.Payments
            .FirstOrDefault(p => p.BookingId == evt.BookingId);

        if (existing != null) return;

        var payment = new PaymentEntity
        {
            BookingId = evt.BookingId,
            UserId = evt.UserId,
            Amount = evt.TotalAmount,
            Status = "PENDING",
            Mode = "PENDING",
            Currency = "INR",
            Description = $"Parking fee for Booking #{evt.BookingId}",
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Payment record created for Booking {BookingId} Amount=₹{Amount}",
            evt.BookingId, evt.TotalAmount);
    }
}
