namespace ParkEase.ParkingLot.Events.Published;

public class LotDeletedEvent
{
    public int LotId { get; set; }
    public int ManagerId { get; set; }
    public DateTime DeletedAt { get; set; }
}
