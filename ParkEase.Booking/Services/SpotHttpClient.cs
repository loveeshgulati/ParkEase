using System.Text.Json;
using ParkEase.Booking.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ParkEase.Booking.Services;

public class SpotHttpClient : ISpotHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SpotHttpClient> _logger;

    public SpotHttpClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<SpotHttpClient> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    // ── Add this private helper ───────────────────────────────────────────────
    private void AttachToken()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
            .ToString().Replace("Bearer ", "");

        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<SpotInfo?> GetSpotAsync(int spotId)
    {
        try
        {
            AttachToken(); // ← add this
            var response = await _httpClient.GetAsync($"/api/v1/spots/{spotId}");
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<SpotApiResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get spot {SpotId}", spotId);
            return null;
        }
    }

    public async Task<bool> ReserveSpotAsync(int spotId)
    {
        try
        {
            AttachToken(); // ← add this
            var response = await _httpClient.PutAsync(
                $"/api/v1/spots/{spotId}/reserve", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reserve spot {SpotId}", spotId);
            return false;
        }
    }

    public async Task<bool> OccupySpotAsync(int spotId)
    {
        try
        {
            AttachToken(); // ← add this
            var response = await _httpClient.PutAsync(
                $"/api/v1/spots/{spotId}/occupy", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to occupy spot {SpotId}", spotId);
            return false;
        }
    }

    public async Task<bool> ReleaseSpotAsync(int spotId)
    {
        try
        {
            AttachToken(); // ← add this
            var response = await _httpClient.PutAsync(
                $"/api/v1/spots/{spotId}/release", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to release spot {SpotId}", spotId);
            return false;
        }
    }

    // ── Inner class for deserialization ───────────────────────────────────────
    private class SpotApiResponse
    {
        public bool Success { get; set; }
        public SpotInfo? Data { get; set; }
    }
}
