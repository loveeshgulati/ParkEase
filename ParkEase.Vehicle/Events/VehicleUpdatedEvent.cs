namespace ParkEase.Vehicle.Events;

// Published by Vehicle-Service

public class VehicleUpdatedEvent
{
    public int VehicleId { get; set; }
    public int OwnerId { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public bool IsEV { get; set; }
    public DateTime UpdatedAt { get; set; }
}
