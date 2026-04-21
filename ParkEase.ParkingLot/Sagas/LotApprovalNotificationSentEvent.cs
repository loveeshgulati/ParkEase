namespace ParkEase.ParkingLot.Sagas;

// Response Events from other services

public class LotApprovalNotificationSentEvent
{
    public Guid SagaCorrelationId { get; set; }
    public int LotId { get; set; }
}
