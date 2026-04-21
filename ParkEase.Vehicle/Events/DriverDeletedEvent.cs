namespace ParkEase.Vehicle.Events;

// Consumed from Auth-Service

/// <summary>
/// When admin deletes a driver from auth-service,
/// vehicle-service cascades and deletes all their vehicles.
/// </summary>
public class DriverDeletedEvent
{
    public int DriverId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }
}
