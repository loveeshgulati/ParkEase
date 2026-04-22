namespace ParkEase.ParkingLot.Entities;

public class ParkingLot
{
    public int LotId { get; set; }
    public int ManagerId { get; set; }              // links to User in auth-service
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int TotalSpots { get; set; } = 0;
    public int AvailableSpots { get; set; } = 0;
    public bool IsOpen { get; set; } = false;
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }

    // PENDING_APPROVAL | APPROVED | REJECTED
    public string ApprovalStatus { get; set; } = "PENDING_APPROVAL";
    public string? RejectionReason { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedByAdminId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public override string ToString() =>
        $"ParkingLot[{LotId}] {Name} @ {City} Manager={ManagerId} Status={ApprovalStatus}";
}
