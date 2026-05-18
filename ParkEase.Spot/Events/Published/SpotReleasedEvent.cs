namespace ParkEase.Spot.Events.Published;





public class SpotReleasedEvent
{
    public int LotId { get; set; }
    public int SpotId { get; set; }
    public DateTime ReleasedAt { get; set; }
}
