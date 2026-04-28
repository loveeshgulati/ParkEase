namespace ParkEase.Booking.Interfaces;

/// <summary>
/// Handles HTTP calls to other microservices via IHttpClientFactory.
/// Booking-service needs to call Spot-service and ParkingLot-service.
/// </summary>
public interface ISpotHttpClient
{
    Task<SpotInfo?> GetSpotAsync(int spotId);
    Task<bool> ReserveSpotAsync(int spotId);
    Task<bool> OccupySpotAsync(int spotId);
    Task<bool> ReleaseSpotAsync(int spotId);
}

public class SpotInfo
{
    public int SpotId { get; set; }
    public int LotId { get; set; }
    public string Status { get; set; } = string.Empty;
    public double PricePerHour { get; set; }
    public string SpotType { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public bool IsEVCharging { get; set; }
}
