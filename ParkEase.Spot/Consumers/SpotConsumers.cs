using MassTransit;
using ParkEase.Spot.Events.Consumed;
using ParkEase.Spot.Interfaces;

namespace ParkEase.Spot.Consumers;

/// <summary>
/// When a lot is deleted from parkinglot-service,
/// cascade delete ALL spots in that lot.
/// </summary>
public class LotDeletedConsumer : IConsumer<LotDeletedEvent>
{
    private readonly ISpotService _spotService;
    private readonly ILogger<LotDeletedConsumer> _logger;

    public LotDeletedConsumer(ISpotService spotService, ILogger<LotDeletedConsumer> logger)
    {
        _spotService = spotService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<LotDeletedEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation(
            "Lot {LotId} deleted. Cascading spot deletion.", evt.LotId);

        await _spotService.DeleteAllByLotIdAsync(evt.LotId);

        _logger.LogInformation(
            "All spots deleted for Lot {LotId}.", evt.LotId);
    }
}
