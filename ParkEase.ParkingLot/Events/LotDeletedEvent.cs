namespace ParkEase.ParkingLot.Events;

// Published by ParkingLot-Service

public class LotDeletedEvent
{
    public int LotId { get; set; }
    public int ManagerId { get; set; }
    public DateTime DeletedAt { get; set; }
}
