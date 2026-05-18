namespace ParkEase.ParkingLot.Events.Consumed;




public class SpotOccupiedEvent
{
    public int LotId { get; set; }
    public int SpotId { get; set; }
}
