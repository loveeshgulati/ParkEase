namespace ParkEase.ParkingLot.Events.Published;

public class LotStatusChangedEvent
{
    public int LotId { get; set; }
    public int ManagerId { get; set; }
    public bool IsOpen { get; set; }
    public DateTime ChangedAt { get; set; }
}
