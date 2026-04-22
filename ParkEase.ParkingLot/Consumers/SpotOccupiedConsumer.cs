using MassTransit;
using ParkEase.ParkingLot.Events.Consumed;
using ParkEase.ParkingLot.Interfaces;

namespace ParkEase.ParkingLot.Consumers;

/// <summary>
/// When a spot is occupied (check-in), decrement available spots in lot.
/// </summary>
public class SpotOccupiedConsumer : IConsumer<SpotOccupiedEvent>
{
    private readonly IParkingLotRepository _repository;
    private readonly ILogger<SpotOccupiedConsumer> _logger;

    public SpotOccupiedConsumer(
        IParkingLotRepository repository,
        ILogger<SpotOccupiedConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SpotOccupiedEvent> context)
    {
        var evt = context.Message;
        await _repository.DecrementAvailableSpotsAsync(evt.LotId);

        _logger.LogInformation(
            "Spot {SpotId} occupied. Available spots decremented for Lot {LotId}",
            evt.SpotId, evt.LotId);
    }
}
