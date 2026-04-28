namespace ParkEase.Spot.Events.Published;

/// <summary>
/// Published after bulk or single add to sync lot spot counts
/// in parkinglot-service.
/// </summary>
public class LotSpotCountUpdatedEvent
{
    public int LotId { get; set; }
    public int TotalSpots { get; set; }
    public int AvailableSpots { get; set; }
}
