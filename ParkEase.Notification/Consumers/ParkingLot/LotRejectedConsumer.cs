using MassTransit;
using ParkEase.Notification.DTOs.Requests;
using ParkEase.Notification.Events.ParkingLot;
using ParkEase.Notification.Interfaces;

namespace ParkEase.Notification.Consumers.ParkingLot;

public class LotRejectedConsumer : IConsumer<LotRejectedEvent>
{
    private readonly INotificationService _service;
    public LotRejectedConsumer(INotificationService service) => _service = service;

    public async Task Consume(ConsumeContext<LotRejectedEvent> context)
    {
        var e = context.Message;
        await _service.SendAsync(new SendNotificationDto
        {
            RecipientId = e.ManagerId,
            Title = "Lot Registration Rejected",
            Message = $"Your lot '{e.LotName}' was rejected. Reason: {e.Reason}",
            Type = "REJECTION",
            RelatedId = e.LotId,
            RelatedType = "LOT",
            Channel = "APP"
        });
    }
}
