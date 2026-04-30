using MassTransit;
using ParkEase.Notification.DTOs.Requests;
using ParkEase.ParkingLot.Events.Published;
using ParkEase.Notification.Interfaces;

namespace ParkEase.Notification.Consumers.ParkingLot;

public class LotCreatedConsumer : IConsumer<LotCreatedEvent>
{
    private readonly INotificationService _service;
    public LotCreatedConsumer(INotificationService service) => _service = service;

    public async Task Consume(ConsumeContext<LotCreatedEvent> context)
    {
        var e = context.Message;
        
        await _service.SendAsync(new SendNotificationDto
        {
            RecipientId = 1, // Platform Admin
            Title = "New Parking Lot Pending Approval 🏢",
            Message = $"Manager (ID: {e.ManagerId}) has submitted a new lot '{e.Name}' in {e.City} for approval.",
            Type = "SYSTEM",
            RelatedId = e.LotId,
            RelatedType = "PARKING_LOT"
        });
    }
}
