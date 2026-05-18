namespace ParkEase.Spot.Events.Published;





public class LotSpotCountUpdatedEvent
{
    public int LotId { get; set; }
    public int TotalSpots { get; set; }
    public int AvailableSpots { get; set; }
}
