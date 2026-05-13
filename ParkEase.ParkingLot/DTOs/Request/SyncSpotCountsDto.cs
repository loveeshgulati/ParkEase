namespace ParkEase.ParkingLot.DTOs.Request;

public class SyncSpotCountsDto
{
    public int LotId { get; set; }
    public int TotalSpots { get; set; }
    public int AvailableSpots { get; set; }
}
