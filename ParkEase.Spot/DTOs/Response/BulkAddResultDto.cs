namespace ParkEase.Spot.DTOs.Response;

public class BulkAddResultDto
{
    public int LotId { get; set; }
    public int SpotsCreated { get; set; }
    public List<SpotDto> Spots { get; set; } = new();
}
