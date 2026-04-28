using ParkEase.ParkingLot.DTOs.Request;
using ParkEase.ParkingLot.DTOs.Response;

namespace ParkEase.ParkingLot.Interfaces;

public interface IParkingLotService
{
    // ── Manager actions ───────────────────────────────────────────────────────
    Task<LotDto> CreateLotAsync(int managerId, CreateLotDto request);
    Task<LotDto> UpdateLotAsync(int lotId, int managerId, UpdateLotDto request);
    Task DeleteLotAsync(int lotId, int managerId, string role);
    Task<LotDto> ToggleOpenAsync(int lotId, int managerId, string role);
    Task<List<LotDto>> GetLotsByManagerAsync(int managerId);

    // ── Admin actions ─────────────────────────────────────────────────────────
    Task<LotDto> ApproveLotAsync(int lotId, int adminId);
    Task<LotDto> RejectLotAsync(int lotId, int adminId, string reason);
    Task<List<LotDto>> GetPendingLotsAsync();
    Task<List<LotDto>> GetAllLotsAsync();

    // ── Public / Driver actions ───────────────────────────────────────────────
    Task<LotDto> GetLotByIdAsync(int lotId);
    Task<List<LotDto>> SearchLotsByCityAsync(string city);
    Task<List<NearbyLotDto>> GetNearbyLotsAsync(
        double latitude, double longitude,
        double radiusKm = 5.0);

    // ── Internal (called by spot-service) ─────────────────────────────────────
    Task IncrementAvailableSpotsAsync(int lotId);
    Task DecrementAvailableSpotsAsync(int lotId);
    Task UpdateSpotCountsAsync(int lotId, int totalSpots, int availableSpots);
}
