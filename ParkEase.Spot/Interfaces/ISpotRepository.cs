using ParkEase.Spot.Entities;

namespace ParkEase.Spot.Interfaces;

public interface ISpotRepository
{
    Task<ParkingSpot?> FindBySpotIdAsync(int spotId);
    Task<List<ParkingSpot>> FindByLotIdAsync(int lotId);
    Task<List<ParkingSpot>> FindByLotIdAndStatusAsync(int lotId, string status);
    Task<List<ParkingSpot>> FindByLotIdAndSpotTypeAsync(int lotId, string spotType);
    Task<List<ParkingSpot>> FindByLotIdAndVehicleTypeAsync(int lotId, string vehicleType);
    Task<List<ParkingSpot>> FindByLotIdAndFloorAsync(int lotId, int floor);
    Task<List<ParkingSpot>> FindByIsEVChargingAsync(int lotId, bool isEV);
    Task<List<ParkingSpot>> FindByIsHandicappedAsync(int lotId, bool isHandicapped);
    Task<int> CountByLotIdAndStatusAsync(int lotId, string status);
    Task<int> CountByLotIdAsync(int lotId);
    Task<ParkingSpot> CreateAsync(ParkingSpot spot);
    Task<List<ParkingSpot>> CreateBulkAsync(List<ParkingSpot> spots);
    Task<ParkingSpot> UpdateAsync(ParkingSpot spot);
    Task DeleteBySpotIdAsync(int spotId);
    Task DeleteAllByLotIdAsync(int lotId);
    Task<bool> ExistsBySpotNumberAndLotAsync(string spotNumber, int lotId);
}
