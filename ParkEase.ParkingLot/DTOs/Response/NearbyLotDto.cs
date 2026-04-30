namespace ParkEase.ParkingLot.DTOs.Response;

public class NearbyLotDto
{
    public int LotId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double DistanceKm { get; set; }           // calculated distance
    public int AvailableSpots { get; set; }
    public int TotalSpots { get; set; }
    public bool IsOpen { get; set; }
    public string OpenTime { get; set; } = string.Empty;
    public string CloseTime { get; set; } = string.Empty;
}
