namespace ParkEase.ParkingLot.Events;

// Consumed from Spot-Service

/// <summary>
/// When a spot is released (checkout/cancellation), increment available spots.
/// </summary>
public class SpotReleasedEvent
{
    public int LotId { get; set; }
    public int SpotId { get; set; }
}
