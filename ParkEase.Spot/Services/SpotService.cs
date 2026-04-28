using MassTransit;
using ParkEase.Spot.DTOs.Request;
using ParkEase.Spot.DTOs.Response;
using ParkEase.Spot.Entities;
using ParkEase.Spot.Events.Published;
using ParkEase.Spot.Interfaces;

namespace ParkEase.Spot.Services;

public class SpotService : ISpotService
{
    private readonly ISpotRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<SpotService> _logger;

    private static readonly string[] ValidSpotTypes =
        { "COMPACT", "STANDARD", "LARGE", "MOTORBIKE", "EV" };

    private static readonly string[] ValidVehicleTypes =
        { "2W", "4W", "HEAVY" };

    public SpotService(
        ISpotRepository repository,
        IPublishEndpoint publishEndpoint,
        ILogger<SpotService> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    // ── Add Single Spot (Manager) ─────────────────────────────────────────────
    public async Task<SpotDto> AddSpotAsync(int managerId, AddSpotDto request)
    {
        ValidateSpotTypes(request.SpotType, request.VehicleType);

        if (await _repository.ExistsBySpotNumberAndLotAsync(request.SpotNumber, request.LotId))
            throw new InvalidOperationException(
                $"Spot '{request.SpotNumber}' already exists in lot {request.LotId}.");

        var spot = new ParkingSpot
        {
            LotId = request.LotId,
            SpotNumber = request.SpotNumber.ToUpper(),
            Floor = request.Floor,
            SpotType = request.SpotType.ToUpper(),
            VehicleType = request.VehicleType.ToUpper(),
            PricePerHour = request.PricePerHour,
            IsHandicapped = request.IsHandicapped,
            IsEVCharging = request.IsEVCharging,
            Status = "AVAILABLE",
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(spot);

        // Sync lot spot counts
        await SyncLotSpotCountsAsync(request.LotId);

        await _publishEndpoint.Publish(new SpotAddedEvent
        {
            SpotId = created.SpotId,
            LotId = created.LotId,
            SpotNumber = created.SpotNumber,
            SpotType = created.SpotType,
            VehicleType = created.VehicleType,
            PricePerHour = created.PricePerHour,
            IsEVCharging = created.IsEVCharging,
            IsHandicapped = created.IsHandicapped
        });

        _logger.LogInformation(
            "Spot {SpotNumber} added to Lot {LotId}", created.SpotNumber, created.LotId);

        return MapToDto(created);
    }

    // ── Bulk Add Spots (Manager) ──────────────────────────────────────────────
    public async Task<BulkAddResultDto> AddBulkSpotsAsync(int managerId, BulkAddSpotDto request)
    {
        ValidateSpotTypes(request.SpotType, request.VehicleType);

        if (request.Count <= 0 || request.Count > 200)
            throw new InvalidOperationException("Count must be between 1 and 200.");

        var spots = new List<ParkingSpot>();

        for (int i = 1; i <= request.Count; i++)
        {
            var spotNumber = $"{request.Prefix.ToUpper()}-{i:D2}";

            // Skip if already exists
            if (await _repository.ExistsBySpotNumberAndLotAsync(spotNumber, request.LotId))
                continue;

            spots.Add(new ParkingSpot
            {
                LotId = request.LotId,
                SpotNumber = spotNumber,
                Floor = request.Floor,
                SpotType = request.SpotType.ToUpper(),
                VehicleType = request.VehicleType.ToUpper(),
                PricePerHour = request.PricePerHour,
                IsHandicapped = request.IsHandicapped,
                IsEVCharging = request.IsEVCharging,
                Status = "AVAILABLE",
                CreatedAt = DateTime.UtcNow
            });
        }

        var created = await _repository.CreateBulkAsync(spots);

        // Sync lot spot counts
        await SyncLotSpotCountsAsync(request.LotId);

        _logger.LogInformation(
            "{Count} spots bulk added to Lot {LotId}", created.Count, request.LotId);

        return new BulkAddResultDto
        {
            LotId = request.LotId,
            SpotsCreated = created.Count,
            Spots = created.Select(MapToDto).ToList()
        };
    }

    // ── Update Spot (Manager) ─────────────────────────────────────────────────
    public async Task<SpotDto> UpdateSpotAsync(
        int spotId, int managerId, UpdateSpotDto request)
    {
        var spot = await GetAndValidateSpotAsync(spotId);

        if (!string.IsNullOrWhiteSpace(request.SpotType))
        {
            var type = request.SpotType.ToUpper();
            if (!ValidSpotTypes.Contains(type))
                throw new InvalidOperationException($"Invalid spot type '{type}'.");
            spot.SpotType = type;
        }

        if (!string.IsNullOrWhiteSpace(request.VehicleType))
        {
            var vType = request.VehicleType.ToUpper();
            if (!ValidVehicleTypes.Contains(vType))
                throw new InvalidOperationException($"Invalid vehicle type '{vType}'.");
            spot.VehicleType = vType;
        }

        if (request.PricePerHour.HasValue) spot.PricePerHour = request.PricePerHour.Value;
        if (request.IsHandicapped.HasValue) spot.IsHandicapped = request.IsHandicapped.Value;
        if (request.IsEVCharging.HasValue) spot.IsEVCharging = request.IsEVCharging.Value;

        var updated = await _repository.UpdateAsync(spot);

        _logger.LogInformation("Spot {SpotId} updated", spotId);
        return MapToDto(updated);
    }

    // ── Delete Spot (Manager/Admin) ───────────────────────────────────────────
    public async Task DeleteSpotAsync(int spotId, int managerId, string role)
    {
        var spot = await GetAndValidateSpotAsync(spotId);

        if (spot.Status != "AVAILABLE")
            throw new InvalidOperationException(
                "Cannot delete a spot that is currently reserved or occupied.");

        var lotId = spot.LotId;
        await _repository.DeleteBySpotIdAsync(spotId);

        // Sync lot spot counts after deletion
        await SyncLotSpotCountsAsync(lotId);

        await _publishEndpoint.Publish(new SpotDeletedEvent
        {
            SpotId = spotId,
            LotId = lotId,
            DeletedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Spot {SpotId} deleted from Lot {LotId}", spotId, lotId);
    }

    // ── Get Spot By Id ────────────────────────────────────────────────────────
    public async Task<SpotDto> GetSpotByIdAsync(int spotId)
    {
        var spot = await GetAndValidateSpotAsync(spotId);
        return MapToDto(spot);
    }

    // ── Get All Spots In Lot ──────────────────────────────────────────────────
    public async Task<List<SpotDto>> GetSpotsByLotAsync(int lotId)
    {
        var spots = await _repository.FindByLotIdAsync(lotId);
        return spots.Select(MapToDto).ToList();
    }

    // ── Get Available Spots ───────────────────────────────────────────────────
    public async Task<List<SpotDto>> GetAvailableSpotsByLotAsync(int lotId)
    {
        var spots = await _repository.FindByLotIdAndStatusAsync(lotId, "AVAILABLE");
        return spots.Select(MapToDto).ToList();
    }

    // ── Get Spots By Type ─────────────────────────────────────────────────────
    public async Task<List<SpotDto>> GetSpotsByTypeAndLotAsync(int lotId, string spotType)
    {
        var spots = await _repository.FindByLotIdAndSpotTypeAsync(
            lotId, spotType.ToUpper());
        return spots.Select(MapToDto).ToList();
    }

    // ── Get Spots By Vehicle Type ─────────────────────────────────────────────
    public async Task<List<SpotDto>> GetSpotsByVehicleTypeAsync(int lotId, string vehicleType)
    {
        var spots = await _repository.FindByLotIdAndVehicleTypeAsync(
            lotId, vehicleType.ToUpper());
        return spots.Select(MapToDto).ToList();
    }

    // ── Get Spots By Floor ────────────────────────────────────────────────────
    public async Task<List<SpotDto>> GetSpotsByFloorAsync(int lotId, int floor)
    {
        var spots = await _repository.FindByLotIdAndFloorAsync(lotId, floor);
        return spots.Select(MapToDto).ToList();
    }

    // ── Get EV Spots ──────────────────────────────────────────────────────────
    public async Task<List<SpotDto>> GetEVSpotsByLotAsync(int lotId)
    {
        var spots = await _repository.FindByIsEVChargingAsync(lotId, true);
        return spots.Select(MapToDto).ToList();
    }

    // ── Get Handicapped Spots ─────────────────────────────────────────────────
    public async Task<List<SpotDto>> GetHandicappedSpotsByLotAsync(int lotId)
    {
        var spots = await _repository.FindByIsHandicappedAsync(lotId, true);
        return spots.Select(MapToDto).ToList();
    }

    // ── Count Available ───────────────────────────────────────────────────────
    public async Task<int> CountAvailableAsync(int lotId) =>
        await _repository.CountByLotIdAndStatusAsync(lotId, "AVAILABLE");

    // ── Reserve Spot (AVAILABLE → RESERVED) ──────────────────────────────────
    public async Task<SpotDto> ReserveSpotAsync(int spotId)
    {
        var spot = await GetAndValidateSpotAsync(spotId);

        if (spot.Status != "AVAILABLE")
            throw new InvalidOperationException(
                $"Spot {spotId} is not available. Current status: {spot.Status}");

        var oldStatus = spot.Status;
        spot.Status = "RESERVED";
        var updated = await _repository.UpdateAsync(spot);

        await _publishEndpoint.Publish(new SpotStatusChangedEvent
        {
            SpotId = spotId,
            LotId = spot.LotId,
            OldStatus = oldStatus,
            NewStatus = "RESERVED",
            ChangedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Spot {SpotId} reserved", spotId);
        return MapToDto(updated);
    }

    // ── Occupy Spot (RESERVED → OCCUPIED) ────────────────────────────────────
    public async Task<SpotDto> OccupySpotAsync(int spotId)
    {
        var spot = await GetAndValidateSpotAsync(spotId);

        if (spot.Status != "RESERVED")
            throw new InvalidOperationException(
                $"Spot {spotId} must be RESERVED before occupying.");

        var oldStatus = spot.Status;
        spot.Status = "OCCUPIED";
        var updated = await _repository.UpdateAsync(spot);

        // Notify parkinglot-service to decrement available spots
        await _publishEndpoint.Publish(new SpotOccupiedEvent
        {
            LotId = spot.LotId,
            SpotId = spotId,
            OccupiedAt = DateTime.UtcNow
        });

        await _publishEndpoint.Publish(new SpotStatusChangedEvent
        {
            SpotId = spotId,
            LotId = spot.LotId,
            OldStatus = oldStatus,
            NewStatus = "OCCUPIED",
            ChangedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Spot {SpotId} occupied", spotId);
        return MapToDto(updated);
    }

    // ── Release Spot (OCCUPIED/RESERVED → AVAILABLE) ──────────────────────────
    public async Task<SpotDto> ReleaseSpotAsync(int spotId)
    {
        var spot = await GetAndValidateSpotAsync(spotId);

        if (spot.Status == "AVAILABLE")
            throw new InvalidOperationException($"Spot {spotId} is already available.");

        var oldStatus = spot.Status;
        spot.Status = "AVAILABLE";
        var updated = await _repository.UpdateAsync(spot);

        // Notify parkinglot-service to increment available spots
        await _publishEndpoint.Publish(new SpotReleasedEvent
        {
            LotId = spot.LotId,
            SpotId = spotId,
            ReleasedAt = DateTime.UtcNow
        });

        await _publishEndpoint.Publish(new SpotStatusChangedEvent
        {
            SpotId = spotId,
            LotId = spot.LotId,
            OldStatus = oldStatus,
            NewStatus = "AVAILABLE",
            ChangedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Spot {SpotId} released", spotId);
        return MapToDto(updated);
    }

    // ── Cascade Delete ────────────────────────────────────────────────────────
    public async Task DeleteAllByLotIdAsync(int lotId)
    {
        await _repository.DeleteAllByLotIdAsync(lotId);
        _logger.LogInformation("All spots deleted for Lot {LotId}", lotId);
    }

    // ── Private Helpers ───────────────────────────────────────────────────────
    private async Task<ParkingSpot> GetAndValidateSpotAsync(int spotId) =>
        await _repository.FindBySpotIdAsync(spotId)
            ?? throw new KeyNotFoundException($"Spot {spotId} not found.");

    private void ValidateSpotTypes(string spotType, string vehicleType)
    {
        if (!ValidSpotTypes.Contains(spotType.ToUpper()))
            throw new InvalidOperationException(
                $"Invalid spot type '{spotType}'. Must be: {string.Join(", ", ValidSpotTypes)}");

        if (!ValidVehicleTypes.Contains(vehicleType.ToUpper()))
            throw new InvalidOperationException(
                $"Invalid vehicle type '{vehicleType}'. Must be: {string.Join(", ", ValidVehicleTypes)}");
    }

    private async Task SyncLotSpotCountsAsync(int lotId)
    {
        var total = await _repository.CountByLotIdAsync(lotId);
        var available = await _repository.CountByLotIdAndStatusAsync(lotId, "AVAILABLE");

        await _publishEndpoint.Publish(new LotSpotCountUpdatedEvent
        {
            LotId = lotId,
            TotalSpots = total,
            AvailableSpots = available
        });
    }

    public static SpotDto MapToDto(ParkingSpot s) => new()
    {
        SpotId = s.SpotId,
        LotId = s.LotId,
        SpotNumber = s.SpotNumber,
        Floor = s.Floor,
        SpotType = s.SpotType,
        VehicleType = s.VehicleType,
        Status = s.Status,
        IsHandicapped = s.IsHandicapped,
        IsEVCharging = s.IsEVCharging,
        PricePerHour = s.PricePerHour,
        CreatedAt = s.CreatedAt
    };
}
