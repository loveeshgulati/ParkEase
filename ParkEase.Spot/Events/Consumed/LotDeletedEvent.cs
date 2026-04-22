namespace ParkEase.Spot.Events.Consumed;

/// <summary>
/// When a lot is deleted, cascade delete all its spots.
/// </summary>
public class LotDeletedEvent
{
    public int LotId { get; set; }
    public int ManagerId { get; set; }
    public DateTime DeletedAt { get; set; }
}
