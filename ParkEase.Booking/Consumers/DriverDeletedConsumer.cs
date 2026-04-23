using MassTransit;
using ParkEase.Booking.Events.Consumed;
using ParkEase.Booking.Interfaces;

namespace ParkEase.Booking.Consumers;

/// <summary>
/// When admin permanently deletes a driver,
/// cancel ALL their bookings (active + reserved).
/// </summary>
public class DriverDeletedConsumer : IConsumer<DriverDeletedEvent>
{
    private readonly IBookingRepository _repository;
    private readonly ISpotHttpClient _spotHttpClient;
    private readonly ILogger<DriverDeletedConsumer> _logger;

    public DriverDeletedConsumer(
        IBookingRepository repository,
        ISpotHttpClient spotHttpClient,
        ILogger<DriverDeletedConsumer> logger)
    {
        _repository = repository;
        _spotHttpClient = spotHttpClient;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DriverDeletedEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation(
            "Driver {DriverId} deleted. Cancelling their bookings.", evt.DriverId);

        var bookings = await _repository.FindByUserIdAsync(evt.DriverId);
        var active = bookings
            .Where(b => b.Status == "RESERVED" || b.Status == "ACTIVE")
            .ToList();

        foreach (var booking in active)
        {
            booking.Status = "CANCELLED";
            booking.CancellationReason = "Driver account deleted";
            await _repository.UpdateAsync(booking);
            await _spotHttpClient.ReleaseSpotAsync(booking.SpotId);
        }

        _logger.LogInformation(
            "{Count} bookings cancelled for deleted Driver {DriverId}",
            active.Count, evt.DriverId);
    }
}
