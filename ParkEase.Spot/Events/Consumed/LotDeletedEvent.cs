namespace ParkEase.Spot.Events.Consumed;




public class LotDeletedEvent
{
    public int LotId { get; set; }
    public int ManagerId { get; set; }
    public DateTime DeletedAt { get; set; }
}
