namespace ParkEase.Spot.Events.Published;

/// <summary>
/// Published when spot transitions to OCCUPIED (check-in).
/// Consumed by parkinglot-service to decrement available spots.
/// </summary>
public class SpotOccupiedEvent
{
    public int LotId { get; set; }
    public int SpotId { get; set; }
    public DateTime OccupiedAt { get; set; }
}
