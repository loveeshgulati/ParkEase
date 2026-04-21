namespace ParkEase.ParkingLot.Events;

// Consumed from Spot-Service

/// <summary>
/// When spots are added/removed, update lot spot counts.
/// </summary>
public class LotSpotCountUpdatedEvent
{
    public int LotId { get; set; }
    public int TotalSpots { get; set; }
    public int AvailableSpots { get; set; }
}
