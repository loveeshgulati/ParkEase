namespace ParkEase.Auth.DTOs;

public class DriverDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? VehiclePlate { get; set; }
    public DateTime CreatedAt { get; set; }
}
