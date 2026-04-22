namespace ParkEase.Spot.Events.Published;

/// <summary>
/// Published when spot transitions back to AVAILABLE (checkout/cancel).
/// Consumed by parkinglot-service to increment available spots.
/// </summary>
public class SpotReleasedEvent
{
    public int LotId { get; set; }
    public int SpotId { get; set; }
    public DateTime ReleasedAt { get; set; }
}
