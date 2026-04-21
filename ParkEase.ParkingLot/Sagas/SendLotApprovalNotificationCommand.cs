namespace ParkEase.ParkingLot.Sagas;

// Commands

public class SendLotApprovalNotificationCommand
{
    public Guid CorrelationId { get; set; }
    public int LotId { get; set; }
    public int ManagerId { get; set; }
    public string LotName { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public string? RejectionReason { get; set; }
}
