using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using ParkEase.Payment.DTOs;

namespace ParkEase.Payment.Services;

public interface IRazorpayService
{
    Task<RazorpayOrderResponse> CreateOrderAsync(decimal amount, string receipt);
    Task<bool> VerifyPaymentAsync(string orderId, string paymentId, string signature);
}

public class RazorpayService : IRazorpayService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly ILogger<RazorpayService> _logger;

    public RazorpayService(
        IConfiguration config,
        HttpClient httpClient,
        ILogger<RazorpayService> logger)
    {
        _config = config;
        _httpClient = httpClient;
        _logger = logger;
        
        var keyId = _config["Razorpay:KeyId"];
        var keySecret = _config["Razorpay:KeySecret"];
        
        _httpClient.BaseAddress = new Uri("https://api.razorpay.com/v1/");
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{keySecret}")));
    }

    public async Task<RazorpayOrderResponse> CreateOrderAsync(decimal amount, string receipt)
    {
        try
        {
            var orderRequest = new
            {
                amount = (int)(amount * 100), // Convert to paise
                currency = "INR",
                receipt = receipt,
                payment_capture = 1
            };

            var content = new StringContent(
                JsonSerializer.Serialize(orderRequest), 
                Encoding.UTF8, 
                "application/json");

            var response = await _httpClient.PostAsync("orders", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var order = JsonSerializer.Deserialize<RazorpayOrderResponse>(
                responseContent, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            _logger.LogInformation($"Razorpay order created: {order?.Id}");
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Razorpay order");
            throw new InvalidOperationException("Failed to create payment order", ex);
        }
    }

    public async Task<bool> VerifyPaymentAsync(string orderId, string paymentId, string signature)
    {
        try
        {
            var keySecret = _config["Razorpay:KeySecret"];
            var generatedSignature = $"{orderId}|{paymentId}";
            
            using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(keySecret));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(generatedSignature));
            var computedSignature = BitConverter.ToString(computedHash).Replace("-", "").ToLower();

            var isValid = computedSignature == signature;
            _logger.LogInformation($"Payment verification: Order={orderId}, Payment={paymentId}, Valid={isValid}");

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify Razorpay payment");
            return false;
        }
    }
}

public class RazorpayOrderResponse
{
    public string Id { get; set; }
    public string Entity { get; set; }
    public int Amount { get; set; }
    public string Currency { get; set; }
    public string Receipt { get; set; }
    public string Status { get; set; }
    public int Attempts { get; set; }
    public DateTime CreatedAt { get; set; }
}
