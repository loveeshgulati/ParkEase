using MassTransit;
using ParkEase.Booking.Events.Consumed;
using ParkEase.Booking.Events.Published;
using ParkEase.Booking.Interfaces;

namespace ParkEase.Booking.Consumers;







public class CancelBookingsForUserConsumer : IConsumer<CancelBookingsForUserCommand>
{
    private readonly IBookingRepository _repository;
    private readonly ISpotHttpClient _spotHttpClient;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CancelBookingsForUserConsumer> _logger;

    public CancelBookingsForUserConsumer(
        IBookingRepository repository,
        ISpotHttpClient spotHttpClient,
        IPublishEndpoint publishEndpoint,
        ILogger<CancelBookingsForUserConsumer> logger)
    {
        _repository = repository;
        _spotHttpClient = spotHttpClient;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CancelBookingsForUserCommand> context)
    {
        var cmd = context.Message;
        _logger.LogInformation(
            "Cancelling bookings for User {UserId} (saga)", cmd.UserId);

        try
        {
            var activeBookings = await _repository.FindByUserIdAsync(cmd.UserId);
            var toCancel = activeBookings
                .Where(b => b.Status == "RESERVED" || b.Status == "ACTIVE")
                .ToList();

            foreach (var booking in toCancel)
            {
                booking.Status = "CANCELLED";
                booking.CancellationReason = "Account deactivated";
                await _repository.UpdateAsync(booking);
                await _spotHttpClient.ReleaseSpotAsync(booking.SpotId);
            }

            
            await _publishEndpoint.Publish(new BookingsCancelledForUserEvent
            {
                SagaCorrelationId = cmd.CorrelationId,
                UserId = cmd.UserId,
                CancelledCount = toCancel.Count
            });

            _logger.LogInformation(
                "{Count} bookings cancelled for User {UserId}",
                toCancel.Count, cmd.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to cancel bookings for User {UserId}", cmd.UserId);

            
            await _publishEndpoint.Publish(new BookingCancellationFailedEvent
            {
                SagaCorrelationId = cmd.CorrelationId,
                UserId = cmd.UserId,
                Reason = ex.Message
            });
        }
    }
}
