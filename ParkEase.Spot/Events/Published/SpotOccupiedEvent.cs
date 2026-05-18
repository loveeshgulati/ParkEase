namespace ParkEase.Spot.Events.Published;





public class SpotOccupiedEvent
{
    public int LotId { get; set; }
    public int SpotId { get; set; }
    public DateTime OccupiedAt { get; set; }
}
