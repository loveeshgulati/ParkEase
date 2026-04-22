namespace ParkEase.ParkingLot.Events.Published;

public class LotApprovedEvent
{
    public int LotId { get; set; }
    public int ManagerId { get; set; }
    public string LotName { get; set; } = string.Empty;
    public int ApprovedByAdminId { get; set; }
    public DateTime ApprovedAt { get; set; }
}
