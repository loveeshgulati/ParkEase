namespace ParkEase.Vehicle.Interfaces;

public interface IVehicleRepository
{
    Task<Entities.Vehicle?> FindByVehicleIdAsync(int vehicleId);
    Task<List<Entities.Vehicle>> FindByOwnerIdAsync(int ownerId);
    Task<Entities.Vehicle?> FindByLicensePlateAndOwnerAsync(string licensePlate, int ownerId);
    Task<bool> ExistsByLicensePlateAndOwnerAsync(string licensePlate, int ownerId);
    Task<List<Entities.Vehicle>> FindByVehicleTypeAsync(string vehicleType);
    Task<List<Entities.Vehicle>> FindByIsEVAsync(bool isEV);
    Task<List<Entities.Vehicle>> GetAllAsync();
    Task<Entities.Vehicle> CreateAsync(Entities.Vehicle vehicle);
    Task<Entities.Vehicle> UpdateAsync(Entities.Vehicle vehicle);
    Task DeleteByVehicleIdAsync(int vehicleId);
    Task DeleteAllByOwnerIdAsync(int ownerId);
}
