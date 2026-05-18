namespace ParkEase.ParkingLot.Events.Consumed;




public class SpotReleasedEvent
{
    public int LotId { get; set; }
    public int SpotId { get; set; }
}
