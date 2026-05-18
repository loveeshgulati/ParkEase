namespace ParkEase.Vehicle.Events;



public class VehicleDeletedEvent
{
    public int VehicleId { get; set; }
    public int OwnerId { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }
}
