namespace ParkEase.ParkingLot.Events;

// Published by ParkingLot-Service

public class LotRejectedEvent
{
    public int LotId { get; set; }
    public int ManagerId { get; set; }
    public string LotName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime RejectedAt { get; set; }
}
