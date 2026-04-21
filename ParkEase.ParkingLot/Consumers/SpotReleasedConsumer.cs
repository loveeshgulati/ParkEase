using MassTransit;
using ParkEase.ParkingLot.Events;
using ParkEase.ParkingLot.Interfaces;

namespace ParkEase.ParkingLot.Consumers;

/// <summary>
/// When a spot is released (checkout/cancellation),
/// increment available spots in lot.
/// </summary>
public class SpotReleasedConsumer : IConsumer<SpotReleasedEvent>
{
    private readonly IParkingLotRepository _repository;
    private readonly ILogger<SpotReleasedConsumer> _logger;

    public SpotReleasedConsumer(
        IParkingLotRepository repository,
        ILogger<SpotReleasedConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SpotReleasedEvent> context)
    {
        var evt = context.Message;
        await _repository.IncrementAvailableSpotsAsync(evt.LotId);

        _logger.LogInformation(
            "Spot {SpotId} released. Available spots incremented for Lot {LotId}",
            evt.SpotId, evt.LotId);
    }
}
