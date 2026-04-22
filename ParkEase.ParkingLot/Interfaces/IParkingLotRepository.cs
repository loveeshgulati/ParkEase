namespace ParkEase.ParkingLot.Interfaces;

public interface IParkingLotRepository
{
    Task<Entities.ParkingLot?> FindByLotIdAsync(int lotId);
    Task<List<Entities.ParkingLot>> FindByCityAsync(string city);
    Task<List<Entities.ParkingLot>> FindByManagerIdAsync(int managerId);
    Task<List<Entities.ParkingLot>> FindByApprovalStatusAsync(string status);
    Task<List<Entities.ParkingLot>> FindAllApprovedAndOpenAsync();
    Task<List<Entities.ParkingLot>> GetAllAsync();
    Task<Entities.ParkingLot> CreateAsync(Entities.ParkingLot lot);
    Task<Entities.ParkingLot> UpdateAsync(Entities.ParkingLot lot);
    Task DeleteByLotIdAsync(int lotId);
    Task DeleteAllByManagerIdAsync(int managerId);
    Task IncrementAvailableSpotsAsync(int lotId);
    Task DecrementAvailableSpotsAsync(int lotId);
    Task UpdateSpotCountsAsync(int lotId, int totalSpots, int availableSpots);
}
