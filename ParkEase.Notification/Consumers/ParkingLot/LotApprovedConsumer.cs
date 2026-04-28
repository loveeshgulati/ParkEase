using MassTransit;
using ParkEase.Notification.DTOs.Requests;
using ParkEase.ParkingLot.Events.Published;
using ParkEase.Notification.Interfaces;

namespace ParkEase.Notification.Consumers.ParkingLot;

public class LotApprovedConsumer : IConsumer<LotApprovedEvent>
{
    private readonly INotificationService _service;
    public LotApprovedConsumer(INotificationService service) => _service = service;

    public async Task Consume(ConsumeContext<LotApprovedEvent> context)
    {
        var e = context.Message;
        await _service.SendAsync(new SendNotificationDto
        {
            RecipientId = e.ManagerId,
            Title = "Lot Approved ✅",
            Message = $"Your lot '{e.LotName}' has been approved. You can now add spots and open it.",
            Type = "APPROVAL",
            RelatedId = e.LotId,
            RelatedType = "LOT"
        });
    }
}

