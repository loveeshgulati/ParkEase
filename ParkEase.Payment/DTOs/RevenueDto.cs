namespace ParkEase.Payment.DTOs;

/// <summary>
/// DTO representing revenue data for a parking lot
/// </summary>
public class RevenueDto
{
    public int LotId { get; set; }
    public double TotalRevenue { get; set; }
    public int TotalTransactions { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}
