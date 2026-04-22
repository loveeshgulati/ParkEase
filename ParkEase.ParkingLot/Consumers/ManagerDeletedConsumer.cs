using MassTransit;
using ParkEase.ParkingLot.Events.Consumed;
using ParkEase.ParkingLot.Interfaces;

namespace ParkEase.ParkingLot.Consumers;

/// <summary>
/// When admin deletes a manager from auth-service,
/// cascade delete ALL their parking lots.
/// </summary>
public class ManagerDeletedConsumer : IConsumer<ManagerDeletedEvent>
{
    private readonly IParkingLotRepository _repository;
    private readonly ILogger<ManagerDeletedConsumer> _logger;

    public ManagerDeletedConsumer(
        IParkingLotRepository repository,
        ILogger<ManagerDeletedConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ManagerDeletedEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation(
            "Manager {ManagerId} deleted. Cascading lot deletion.", evt.ManagerId);

        await _repository.DeleteAllByManagerIdAsync(evt.ManagerId);

        _logger.LogInformation(
            "All lots for Manager {ManagerId} deleted.", evt.ManagerId);
    }
}
