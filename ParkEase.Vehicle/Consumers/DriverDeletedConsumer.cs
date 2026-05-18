using MassTransit;
using ParkEase.Vehicle.Events;
using ParkEase.Vehicle.Interfaces;

namespace ParkEase.Vehicle.Consumers;





public class DriverDeletedConsumer : IConsumer<DriverDeletedEvent>
{
    private readonly IVehicleService _vehicleService;
    private readonly ILogger<DriverDeletedConsumer> _logger;

    public DriverDeletedConsumer(
        IVehicleService vehicleService,
        ILogger<DriverDeletedConsumer> logger)
    {
        _vehicleService = vehicleService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DriverDeletedEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation(
            "Driver {DriverId} deleted. Cascading vehicle deletion.", evt.DriverId);

        await _vehicleService.DeleteAllByOwnerIdAsync(evt.DriverId);

        _logger.LogInformation(
            "All vehicles for Driver {DriverId} deleted successfully.", evt.DriverId);
    }
}
