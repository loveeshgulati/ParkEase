namespace ParkEase.ParkingLot.DTOs.Request;

public class UpdateLotDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? OpenTime { get; set; }
    public string? CloseTime { get; set; }
}
