namespace ParkEase.ParkingLot.Events;

// Published by ParkingLot-Service

public class LotCreatedEvent
{
    public int LotId { get; set; }
    public int ManagerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
