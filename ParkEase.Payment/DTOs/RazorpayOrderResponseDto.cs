namespace ParkEase.Payment.DTOs;

/// <summary>Response DTO for POST /api/v1/payments/create-order</summary>
public class RazorpayOrderResponseDto
{
    /// <summary>Razorpay Order ID (e.g. "order_ABC123"). Pass this to the checkout widget.</summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>Razorpay Key ID — safe to expose to the frontend.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Amount in INR rupees (as provided in the request).</summary>
    public double Amount { get; set; }

    /// <summary>Currency code (always "INR").</summary>
    public string Currency { get; set; } = "INR";
}
