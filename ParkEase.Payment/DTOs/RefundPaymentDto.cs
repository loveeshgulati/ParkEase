using System.ComponentModel.DataAnnotations;

namespace ParkEase.Payment.DTOs;

/// <summary>
/// DTO for refunding a payment
/// </summary>
public class RefundPaymentDto
{
    [Required] public int PaymentId { get; set; }
    [Required] public string Reason { get; set; } = string.Empty;
    public double? RefundAmount { get; set; } // null = full refund
}
