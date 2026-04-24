using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ParkEase.Payment.Interfaces;

namespace ParkEase.Payment.Services;

/// <summary>
/// Handles Razorpay order creation (via REST API) and payment signature verification.
/// Uses only the standard .NET HTTP + crypto stack — no Razorpay SDK required.
/// Install the SDK later with:  dotnet add package Razorpay
/// </summary>
public class RazorpayService : IRazorpayService
{
    // Razorpay REST endpoint for order creation
    private const string RazorpayOrdersUrl = "https://api.razorpay.com/v1/orders";

    private readonly string _keyId;
    private readonly string _keySecret;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RazorpayService> _logger;

    public RazorpayService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<RazorpayService> logger)
    {
        _keyId     = configuration["Razorpay:KeyId"]
            ?? throw new InvalidOperationException("Razorpay:KeyId is not configured.");
        _keySecret = configuration["Razorpay:KeySecret"]
            ?? throw new InvalidOperationException("Razorpay:KeySecret is not configured.");
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    // ── Create Order ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<string> CreateOrderAsync(double amountInRupees, string? receipt = null)
    {
        // Razorpay requires amount in paise (1 INR = 100 paise)
        var amountInPaise = (long)Math.Round(amountInRupees * 100);

        var payload = new
        {
            amount   = amountInPaise,
            currency = "INR",
            receipt  = receipt ?? $"rcpt_{Guid.NewGuid():N}"[..36]
        };

        var json    = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var client = _httpClientFactory.CreateClient("Razorpay");

        // Basic auth: KeyId:KeySecret
        var credentials = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{_keyId}:{_keySecret}"));
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

        _logger.LogInformation(
            "Creating Razorpay order for ₹{Amount} ({Paise} paise)",
            amountInRupees, amountInPaise);

        var response = await client.PostAsync(RazorpayOrdersUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "Razorpay order creation failed. Status={Status} Body={Body}",
                response.StatusCode, error);
            throw new InvalidOperationException(
                $"Razorpay order creation failed: {response.StatusCode}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseBody);
        var orderId = doc.RootElement.GetProperty("id").GetString()
            ?? throw new InvalidOperationException(
                "Razorpay response did not contain an order ID.");

        _logger.LogInformation("Razorpay order created: {OrderId}", orderId);
        return orderId;
    }

    // ── Verify Signature ──────────────────────────────────────────────────────

    /// <inheritdoc/>
    public bool VerifySignature(
        string razorpayOrderId,
        string razorpayPaymentId,
        string razorpaySignature)
    {
        // Razorpay signature = HMAC-SHA256(order_id + "|" + payment_id, key_secret)
        var message  = $"{razorpayOrderId}|{razorpayPaymentId}";
        var keyBytes = Encoding.UTF8.GetBytes(_keySecret);
        var msgBytes = Encoding.UTF8.GetBytes(message);

        using var hmac          = new HMACSHA256(keyBytes);
        var computedHashBytes   = hmac.ComputeHash(msgBytes);
        var computedHash        = Convert.ToHexString(computedHashBytes).ToLower();

        var isValid = computedHash == razorpaySignature.ToLower();

        if (isValid)
            _logger.LogInformation(
                "Razorpay signature verified OK for OrderId={OrderId} PaymentId={PaymentId}",
                razorpayOrderId, razorpayPaymentId);
        else
            _logger.LogWarning(
                "Razorpay signature MISMATCH for OrderId={OrderId} PaymentId={PaymentId}",
                razorpayOrderId, razorpayPaymentId);

        return isValid;
    }
}
