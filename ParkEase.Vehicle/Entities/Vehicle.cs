namespace ParkEase.Vehicle.Entities;

public class Vehicle
{
    public int VehicleId { get; set; }
    public int OwnerId { get; set; }              
    public string LicensePlate { get; set; } = string.Empty; 
    public string Make { get; set; } = string.Empty;         
    public string Model { get; set; } = string.Empty;        
    public string Color { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;  
    public bool IsEV { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public override string ToString() =>
        $"Vehicle[{VehicleId}] {Make} {Model} ({LicensePlate}) Owner={OwnerId}";
}
