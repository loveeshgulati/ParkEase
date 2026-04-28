namespace ParkEase.Vehicle.Entities;

public class Vehicle
{
    public int VehicleId { get; set; }
    public int OwnerId { get; set; }              // links to User in auth-service
    public string LicensePlate { get; set; } = string.Empty; // unique per owner
    public string Make { get; set; } = string.Empty;         // Toyota, Honda etc
    public string Model { get; set; } = string.Empty;        // Corolla, Civic etc
    public string Color { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;  // 2W | 4W | HEAVY
    public bool IsEV { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public override string ToString() =>
        $"Vehicle[{VehicleId}] {Make} {Model} ({LicensePlate}) Owner={OwnerId}";
}
