using System.ComponentModel.DataAnnotations;

namespace ParkEase.Payment.DTOs;

/// <summary>
/// DTO for processing a payment
/// </summary>
public class ProcessPaymentDto
{
    [Required] public int BookingId { get; set; }

    // CARD | UPI | WALLET | CASH
    [Required] public string Mode { get; set; } = string.Empty;

    [Required] public double Amount { get; set; }
    public string? TransactionId { get; set; }
    public string? Description { get; set; }
}
