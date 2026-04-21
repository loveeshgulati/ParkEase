using Microsoft.EntityFrameworkCore;
using ParkEase.Vehicle.Data;
using ParkEase.Vehicle.Interfaces;

namespace ParkEase.Vehicle.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly VehicleDbContext _context;

    public VehicleRepository(VehicleDbContext context) => _context = context;

    public async Task<Entities.Vehicle?> FindByVehicleIdAsync(int vehicleId) =>
        await _context.Vehicles.FindAsync(vehicleId);

    public async Task<List<Entities.Vehicle>> FindByOwnerIdAsync(int ownerId) =>
        await _context.Vehicles
            .Where(v => v.OwnerId == ownerId && v.IsActive)
            .ToListAsync();

    public async Task<Entities.Vehicle?> FindByLicensePlateAndOwnerAsync(
        string licensePlate, int ownerId) =>
        await _context.Vehicles.FirstOrDefaultAsync(v =>
            v.LicensePlate == licensePlate.ToUpper() && v.OwnerId == ownerId);

    public async Task<bool> ExistsByLicensePlateAndOwnerAsync(
        string licensePlate, int ownerId) =>
        await _context.Vehicles.AnyAsync(v =>
            v.LicensePlate == licensePlate.ToUpper() && v.OwnerId == ownerId);

    public async Task<List<Entities.Vehicle>> FindByVehicleTypeAsync(string vehicleType) =>
        await _context.Vehicles
            .Where(v => v.VehicleType == vehicleType && v.IsActive)
            .ToListAsync();

    public async Task<List<Entities.Vehicle>> FindByIsEVAsync(bool isEV) =>
        await _context.Vehicles
            .Where(v => v.IsEV == isEV && v.IsActive)
            .ToListAsync();

    public async Task<List<Entities.Vehicle>> GetAllAsync() =>
        await _context.Vehicles.ToListAsync();

    public async Task<Entities.Vehicle> CreateAsync(Entities.Vehicle vehicle)
    {
        vehicle.LicensePlate = vehicle.LicensePlate.ToUpper();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();
        return vehicle;
    }

    public async Task<Entities.Vehicle> UpdateAsync(Entities.Vehicle vehicle)
    {
        _context.Vehicles.Update(vehicle);
        await _context.SaveChangesAsync();
        return vehicle;
    }

    public async Task DeleteByVehicleIdAsync(int vehicleId)
    {
        var vehicle = await _context.Vehicles.FindAsync(vehicleId);
        if (vehicle != null)
        {
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAllByOwnerIdAsync(int ownerId)
    {
        var vehicles = await _context.Vehicles
            .Where(v => v.OwnerId == ownerId)
            .ToListAsync();

        if (vehicles.Any())
        {
            _context.Vehicles.RemoveRange(vehicles);
            await _context.SaveChangesAsync();
        }
    }
}
