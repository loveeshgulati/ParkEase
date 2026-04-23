namespace ParkEase.Payment.DTOs;

/// <summary>
/// DTO representing platform-wide revenue data
/// </summary>
public class PlatformRevenueDto
{
    public double TotalRevenue { get; set; }
    public int TotalTransactions { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}
