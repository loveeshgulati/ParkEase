namespace ParkEase.Payment.DTOs;




public class RevenueDto
{
    public int LotId { get; set; }
    public double TotalRevenue { get; set; }
    public int TotalTransactions { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}
