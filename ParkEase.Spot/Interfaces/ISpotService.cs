using ParkEase.Spot.DTOs.Request;
using ParkEase.Spot.DTOs.Response;

namespace ParkEase.Spot.Interfaces;

public interface ISpotService
{
    
    Task<SpotDto> AddSpotAsync(int managerId, AddSpotDto request);
    Task<BulkAddResultDto> AddBulkSpotsAsync(int managerId, BulkAddSpotDto request);
    Task<SpotDto> UpdateSpotAsync(int spotId, int managerId, UpdateSpotDto request);
    Task DeleteSpotAsync(int spotId, int managerId, string role);

    
    Task<SpotDto> GetSpotByIdAsync(int spotId);
    Task<List<SpotDto>> GetSpotsByLotAsync(int lotId);
    Task<List<SpotDto>> GetAvailableSpotsByLotAsync(int lotId);
    Task<List<SpotDto>> GetSpotsByTypeAndLotAsync(int lotId, string spotType);
    Task<List<SpotDto>> GetSpotsByVehicleTypeAsync(int lotId, string vehicleType);
    Task<List<SpotDto>> GetSpotsByFloorAsync(int lotId, int floor);
    Task<List<SpotDto>> GetEVSpotsByLotAsync(int lotId);
    Task<List<SpotDto>> GetHandicappedSpotsByLotAsync(int lotId);
    Task<int> CountAvailableAsync(int lotId);

    
    Task<SpotDto> ReserveSpotAsync(int spotId);   
    Task<SpotDto> OccupySpotAsync(int spotId);    
    Task<SpotDto> ReleaseSpotAsync(int spotId);   

    
    Task DeleteAllByLotIdAsync(int lotId);
}
