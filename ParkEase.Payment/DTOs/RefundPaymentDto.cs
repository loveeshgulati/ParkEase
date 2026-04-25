using System.ComponentModel.DataAnnotations;

namespace ParkEase.Payment.DTOs;

public class RefundPaymentDto
{
    [Required] public int PaymentId { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "Reason must be between 3 and 500 characters.")]
    public string Reason { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Refund amount must be greater than 0")]
    public double? RefundAmount { get; set; }
}
