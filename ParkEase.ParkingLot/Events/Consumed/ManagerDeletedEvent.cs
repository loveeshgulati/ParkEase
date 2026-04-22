namespace ParkEase.ParkingLot.Events.Consumed;

/// <summary>
/// When admin deletes a manager, cascade delete all their lots.
/// </summary>
public class ManagerDeletedEvent
{
    public int ManagerId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }
}
