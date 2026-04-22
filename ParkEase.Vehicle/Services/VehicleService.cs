using MassTransit;
using ParkEase.Vehicle.DTOs;
using ParkEase.Vehicle.Events;
using ParkEase.Vehicle.Interfaces;

namespace ParkEase.Vehicle.Services;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<VehicleService> _logger;

    // Valid vehicle types
    private static readonly string[] ValidVehicleTypes = { "2W", "4W", "HEAVY" };

    public VehicleService(
        IVehicleRepository vehicleRepository,
        IPublishEndpoint publishEndpoint,
        ILogger<VehicleService> logger)
    {
        _vehicleRepository = vehicleRepository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    // ── Register Vehicle ──────────────────────────────────────────────────────
    public async Task<VehicleDto> RegisterVehicleAsync(int ownerId, RegisterVehicleDto request)
    {
        // Validate vehicle type
        var vehicleType = request.VehicleType.ToUpper();
        if (!ValidVehicleTypes.Contains(vehicleType))
            throw new InvalidOperationException(
                $"Invalid vehicle type '{vehicleType}'. Must be 2W, 4W or HEAVY.");

        // Check duplicate license plate for same owner
        if (await _vehicleRepository.ExistsByLicensePlateAndOwnerAsync(
            request.LicensePlate, ownerId))
            throw new InvalidOperationException(
                $"Vehicle with plate '{request.LicensePlate}' already registered.");

        var vehicle = new Entities.Vehicle
        {
            OwnerId = ownerId,
            LicensePlate = request.LicensePlate.ToUpper(),
            Make = request.Make,
            Model = request.Model,
            Color = request.Color,
            VehicleType = vehicleType,
            IsEV = request.IsEV,
            IsActive = true,
            RegisteredAt = DateTime.UtcNow
        };

        var created = await _vehicleRepository.CreateAsync(vehicle);

        // Publish event for booking-service to keep reference
        await _publishEndpoint.Publish(new VehicleRegisteredEvent
        {
            VehicleId = created.VehicleId,
            OwnerId = created.OwnerId,
            LicensePlate = created.LicensePlate,
            VehicleType = created.VehicleType,
            IsEV = created.IsEV,
            RegisteredAt = created.RegisteredAt
        });

        _logger.LogInformation(
            "Vehicle registered: {Plate} for Owner={OwnerId}", created.LicensePlate, ownerId);

        return MapToDto(created);
    }

    // ── Get Vehicle By Id ─────────────────────────────────────────────────────
    public async Task<VehicleDto> GetVehicleByIdAsync(
        int vehicleId, int requestingUserId, string requestingUserRole)
    {
        var vehicle = await _vehicleRepository.FindByVehicleIdAsync(vehicleId)
            ?? throw new KeyNotFoundException($"Vehicle {vehicleId} not found.");

        // RBAC: driver can only see own vehicles, admin sees all
        if (requestingUserRole != "ADMIN" && vehicle.OwnerId != requestingUserId)
            throw new UnauthorizedAccessException("You can only view your own vehicles.");

        return MapToDto(vehicle);
    }

    // ── Get All Vehicles By Owner ─────────────────────────────────────────────
    public async Task<List<VehicleDto>> GetVehiclesByOwnerAsync(
        int ownerId, int requestingUserId, string requestingUserRole)
    {
        // RBAC: driver can only see own, admin sees any
        if (requestingUserRole != "ADMIN" && ownerId != requestingUserId)
            throw new UnauthorizedAccessException("You can only view your own vehicles.");

        var vehicles = await _vehicleRepository.FindByOwnerIdAsync(ownerId);
        return vehicles.Select(MapToDto).ToList();
    }

    // ── Update Vehicle ────────────────────────────────────────────────────────
    public async Task<VehicleDto> UpdateVehicleAsync(
        int vehicleId, int ownerId, UpdateVehicleDto request)
    {
        var vehicle = await _vehicleRepository.FindByVehicleIdAsync(vehicleId)
            ?? throw new KeyNotFoundException($"Vehicle {vehicleId} not found.");

        // Only owner can update
        if (vehicle.OwnerId != ownerId)
            throw new UnauthorizedAccessException("You can only update your own vehicles.");

        if (!string.IsNullOrWhiteSpace(request.Make)) vehicle.Make = request.Make;
        if (!string.IsNullOrWhiteSpace(request.Model)) vehicle.Model = request.Model;
        if (!string.IsNullOrWhiteSpace(request.Color)) vehicle.Color = request.Color;

        if (!string.IsNullOrWhiteSpace(request.VehicleType))
        {
            var vehicleType = request.VehicleType.ToUpper();
            if (!ValidVehicleTypes.Contains(vehicleType))
                throw new InvalidOperationException(
                    $"Invalid vehicle type '{vehicleType}'. Must be 2W, 4W or HEAVY.");
            vehicle.VehicleType = vehicleType;
        }

        if (request.IsEV.HasValue) vehicle.IsEV = request.IsEV.Value;

        var updated = await _vehicleRepository.UpdateAsync(vehicle);

        // Publish update event for booking-service
        await _publishEndpoint.Publish(new VehicleUpdatedEvent
        {
            VehicleId = updated.VehicleId,
            OwnerId = updated.OwnerId,
            LicensePlate = updated.LicensePlate,
            VehicleType = updated.VehicleType,
            IsEV = updated.IsEV,
            UpdatedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Vehicle {VehicleId} updated by Owner={OwnerId}", vehicleId, ownerId);

        return MapToDto(updated);
    }

    // ── Delete Vehicle ────────────────────────────────────────────────────────
    public async Task DeleteVehicleAsync(int vehicleId, int ownerId, string role)
    {
        var vehicle = await _vehicleRepository.FindByVehicleIdAsync(vehicleId)
            ?? throw new KeyNotFoundException($"Vehicle {vehicleId} not found.");

        // Owner or admin can delete
        if (role != "ADMIN" && vehicle.OwnerId != ownerId)
            throw new UnauthorizedAccessException("You can only delete your own vehicles.");

        await _vehicleRepository.DeleteByVehicleIdAsync(vehicleId);

        await _publishEndpoint.Publish(new VehicleDeletedEvent
        {
            VehicleId = vehicleId,
            OwnerId = vehicle.OwnerId,
            LicensePlate = vehicle.LicensePlate,
            DeletedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Vehicle {VehicleId} deleted", vehicleId);
    }

    // ── Get Vehicle Type ──────────────────────────────────────────────────────
    public async Task<string> GetVehicleTypeAsync(int vehicleId)
    {
        var vehicle = await _vehicleRepository.FindByVehicleIdAsync(vehicleId)
            ?? throw new KeyNotFoundException($"Vehicle {vehicleId} not found.");
        return vehicle.VehicleType;
    }

    // ── Is EV Vehicle ─────────────────────────────────────────────────────────
    public async Task<bool> IsEVVehicleAsync(int vehicleId)
    {
        var vehicle = await _vehicleRepository.FindByVehicleIdAsync(vehicleId)
            ?? throw new KeyNotFoundException($"Vehicle {vehicleId} not found.");
        return vehicle.IsEV;
    }

    // ── Get All Vehicles (Admin) ──────────────────────────────────────────────
    public async Task<List<VehicleDto>> GetAllVehiclesAsync()
    {
        var vehicles = await _vehicleRepository.GetAllAsync();
        return vehicles.Select(MapToDto).ToList();
    }

    // ── Cascade Delete (on driver deletion) ───────────────────────────────────
    public async Task DeleteAllByOwnerIdAsync(int ownerId)
    {
        await _vehicleRepository.DeleteAllByOwnerIdAsync(ownerId);
        _logger.LogInformation("All vehicles deleted for Owner={OwnerId}", ownerId);
    }

    // ── Mapper ────────────────────────────────────────────────────────────────
    public static VehicleDto MapToDto(Entities.Vehicle v) => new()
    {
        VehicleId = v.VehicleId,
        OwnerId = v.OwnerId,
        LicensePlate = v.LicensePlate,
        Make = v.Make,
        Model = v.Model,
        Color = v.Color,
        VehicleType = v.VehicleType,
        IsEV = v.IsEV,
        IsActive = v.IsActive,
        RegisteredAt = v.RegisteredAt
    };
}
