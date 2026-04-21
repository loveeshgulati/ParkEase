using MassTransit;
using ParkEase.ParkingLot.Events;
using ParkEase.ParkingLot.Interfaces;

namespace ParkEase.ParkingLot.Consumers;

/// <summary>
/// When spots are added or removed from a lot,
/// update the lot's total and available spot counts.
/// </summary>
public class LotSpotCountUpdatedConsumer : IConsumer<LotSpotCountUpdatedEvent>
{
    private readonly IParkingLotRepository _repository;
    private readonly ILogger<LotSpotCountUpdatedConsumer> _logger;

    public LotSpotCountUpdatedConsumer(
        IParkingLotRepository repository,
        ILogger<LotSpotCountUpdatedConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<LotSpotCountUpdatedEvent> context)
    {
        var evt = context.Message;
        await _repository.UpdateSpotCountsAsync(
            evt.LotId, evt.TotalSpots, evt.AvailableSpots);

        _logger.LogInformation(
            "Lot {LotId} spot counts updated: Total={Total} Available={Available}",
            evt.LotId, evt.TotalSpots, evt.AvailableSpots);
    }
}
