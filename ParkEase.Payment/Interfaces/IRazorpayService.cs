namespace ParkEase.Payment.Interfaces;

/// <summary>
/// Abstraction over Razorpay API calls and signature verification.
/// Keeps all Razorpay-specific logic out of the controller and PaymentService.
/// </summary>
public interface IRazorpayService
{
    /// <summary>
    /// Creates a Razorpay order and returns the Razorpay order ID.
    /// </summary>
    /// <param name="amountInRupees">Amount in INR (not paise).</param>
    /// <param name="receipt">Optional receipt string (≤ 40 chars).</param>
    Task<string> CreateOrderAsync(double amountInRupees, string? receipt = null);

    /// <summary>
    /// Verifies the HMAC-SHA256 signature sent by Razorpay after payment.
    /// </summary>
    /// <param name="razorpayOrderId">order_id from Razorpay.</param>
    /// <param name="razorpayPaymentId">payment_id from Razorpay.</param>
    /// <param name="razorpaySignature">signature from Razorpay.</param>
    /// <returns>True if the signature is valid.</returns>
    bool VerifySignature(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature);
}
