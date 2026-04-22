namespace ParkEase.Spot.Events.Published;

public class SpotDeletedEvent
{
    public int SpotId { get; set; }
    public int LotId { get; set; }
    public DateTime DeletedAt { get; set; }
}
