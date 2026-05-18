namespace ParkEase.ParkingLot.Events.Consumed;




public class LotSpotCountUpdatedEvent
{
    public int LotId { get; set; }
    public int TotalSpots { get; set; }
    public int AvailableSpots { get; set; }
}
