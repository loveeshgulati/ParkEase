namespace ParkEase.ParkingLot.Events;

// Consumed from Spot-Service

/// <summary>
/// When a spot is occupied (booking check-in), decrement available spots.
/// </summary>
public class SpotOccupiedEvent
{
    public int LotId { get; set; }
    public int SpotId { get; set; }
}
