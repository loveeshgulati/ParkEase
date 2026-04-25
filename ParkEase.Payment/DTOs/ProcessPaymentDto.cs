using System.ComponentModel.DataAnnotations;

namespace ParkEase.Payment.DTOs;

/// <summary>
/// DTO for processing a payment
/// </summary>
public class ProcessPaymentDto
{
    [Required] public int BookingId { get; set; }

    [Required]
    [RegularExpression("^(CARD|UPI|WALLET|CASH)$", ErrorMessage = "Mode must be CARD, UPI, WALLET, or CASH.")]
    public string Mode { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public double Amount { get; set; }

    public string? TransactionId { get; set; }
    public string? Description { get; set; }
}
