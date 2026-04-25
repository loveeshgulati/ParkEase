namespace ParkEase.Notification.Events.ParkingLot;

public class LotApprovedEvent
{
    public int LotId { get; set; }
    public int ManagerId { get; set; }
    public string LotName { get; set; } = string.Empty;
    public DateTime ApprovedAt { get; set; }
}
