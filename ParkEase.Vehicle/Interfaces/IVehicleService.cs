using ParkEase.Vehicle.DTOs;

namespace ParkEase.Vehicle.Interfaces;

public interface IVehicleService
{
    Task<VehicleDto> RegisterVehicleAsync(int ownerId, RegisterVehicleDto request);
    Task<VehicleDto> GetVehicleByIdAsync(int vehicleId, int requestingUserId, string requestingUserRole);
    Task<List<VehicleDto>> GetVehiclesByOwnerAsync(int ownerId, int requestingUserId, string requestingUserRole);
    Task<VehicleDto> UpdateVehicleAsync(int vehicleId, int ownerId, UpdateVehicleDto request);
    Task DeleteVehicleAsync(int vehicleId, int ownerId, string role);
    Task<string> GetVehicleTypeAsync(int vehicleId);
    Task<bool> IsEVVehicleAsync(int vehicleId);
    Task<List<VehicleDto>> GetAllVehiclesAsync();         // admin only
    Task DeleteAllByOwnerIdAsync(int ownerId);            // cascade on driver delete
}
