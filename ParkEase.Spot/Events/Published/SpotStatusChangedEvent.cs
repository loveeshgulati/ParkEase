namespace ParkEase.Spot.Events.Published;

public class SpotStatusChangedEvent
{
    public int SpotId { get; set; }
    public int LotId { get; set; }
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}
