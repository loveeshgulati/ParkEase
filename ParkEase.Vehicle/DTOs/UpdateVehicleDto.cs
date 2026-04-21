namespace ParkEase.Vehicle.DTOs;

public class UpdateVehicleDto
{
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? Color { get; set; }
    public string? VehicleType { get; set; }
    public bool? IsEV { get; set; }
}
