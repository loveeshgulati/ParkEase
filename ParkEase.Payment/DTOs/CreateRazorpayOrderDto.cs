using System.ComponentModel.DataAnnotations;

namespace ParkEase.Payment.DTOs;

/// <summary>Request DTO for POST /api/v1/payments/create-order</summary>
public class CreateRazorpayOrderDto
{
    /// <summary>Amount in INR (rupees). Will be converted to paise internally.</summary>
    [Required]
    [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public double Amount { get; set; }

    /// <summary>Optional receipt reference (e.g. "booking_42")</summary>
    public string? Receipt { get; set; }
}
