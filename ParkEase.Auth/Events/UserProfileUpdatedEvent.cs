namespace ParkEase.Auth.Events;

public class UserProfileUpdatedEvent
{
    public int UserId { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? VehiclePlate { get; set; }
    public DateTime UpdatedAt { get; set; }
}
