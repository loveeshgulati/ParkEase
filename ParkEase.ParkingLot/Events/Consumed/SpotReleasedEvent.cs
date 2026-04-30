namespace ParkEase.ParkingLot.Events.Consumed;

/// <summary>
/// When a spot is released (checkout/cancellation), increment available spots.
/// </summary>
public class SpotReleasedEvent
{
    public int LotId { get; set; }
    public int SpotId { get; set; }
}
