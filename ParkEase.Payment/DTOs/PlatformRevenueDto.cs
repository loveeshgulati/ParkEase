namespace ParkEase.Payment.DTOs;




public class PlatformRevenueDto
{
    public double TotalRevenue { get; set; }
    public int TotalTransactions { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}
