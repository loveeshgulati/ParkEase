using MassTransit;
using ParkEase.ParkingLot.DTOs;
using ParkEase.ParkingLot.Events;
using ParkEase.ParkingLot.Helpers;
using ParkEase.ParkingLot.Interfaces;

namespace ParkEase.ParkingLot.Services;

public class ParkingLotService : IParkingLotService
{
    private readonly IParkingLotRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ParkingLotService> _logger;

    public ParkingLotService(
        IParkingLotRepository repository,
        IPublishEndpoint publishEndpoint,
        ILogger<ParkingLotService> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    // ── Create Lot (Manager) ──────────────────────────────────────────────────
    public async Task<LotDto> CreateLotAsync(int managerId, CreateLotDto request)
    {
        var lot = new Entities.ParkingLot
        {
            ManagerId = managerId,
            Name = request.Name,
            Address = request.Address,
            City = request.City,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            OpenTime = TimeOnly.Parse(request.OpenTime),
            CloseTime = TimeOnly.Parse(request.CloseTime),
            ImageUrl = request.ImageUrl,
            ApprovalStatus = "PENDING_APPROVAL",
            IsOpen = false,
            TotalSpots = 0,
            AvailableSpots = 0,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(lot);

        await _publishEndpoint.Publish(new LotCreatedEvent
        {
            LotId = created.LotId,
            ManagerId = created.ManagerId,
            Name = created.Name,
            City = created.City,
            CreatedAt = created.CreatedAt
        });

        _logger.LogInformation(
            "Lot created: {Name} by Manager={ManagerId} Status=PENDING_APPROVAL",
            created.Name, managerId);

        return MapToDto(created);
    }

    // ── Update Lot (Manager/Admin) ────────────────────────────────────────────
    public async Task<LotDto> UpdateLotAsync(int lotId, int managerId, UpdateLotDto request)
    {
        var lot = await GetAndValidateLotAsync(lotId);

        // Only owner manager or admin can update
        if (lot.ManagerId != managerId)
            throw new UnauthorizedAccessException("You can only update your own lots.");

        if (!string.IsNullOrWhiteSpace(request.Name)) lot.Name = request.Name;
        if (!string.IsNullOrWhiteSpace(request.Address)) lot.Address = request.Address;
        if (!string.IsNullOrWhiteSpace(request.City)) lot.City = request.City;
        if (request.Latitude.HasValue) lot.Latitude = request.Latitude.Value;
        if (request.Longitude.HasValue) lot.Longitude = request.Longitude.Value;
        if (!string.IsNullOrWhiteSpace(request.OpenTime))
            lot.OpenTime = TimeOnly.Parse(request.OpenTime);
        if (!string.IsNullOrWhiteSpace(request.CloseTime))
            lot.CloseTime = TimeOnly.Parse(request.CloseTime);
        if (!string.IsNullOrWhiteSpace(request.ImageUrl)) lot.ImageUrl = request.ImageUrl;

        var updated = await _repository.UpdateAsync(lot);
        _logger.LogInformation("Lot {LotId} updated by Manager={ManagerId}", lotId, managerId);
        return MapToDto(updated);
    }

    // ── Delete Lot (Manager/Admin) ────────────────────────────────────────────
    public async Task DeleteLotAsync(int lotId, int managerId, string role)
    {
        var lot = await GetAndValidateLotAsync(lotId);

        if (role != "ADMIN" && lot.ManagerId != managerId)
            throw new UnauthorizedAccessException("You can only delete your own lots.");

        await _repository.DeleteByLotIdAsync(lotId);

        await _publishEndpoint.Publish(new LotDeletedEvent
        {
            LotId = lotId,
            ManagerId = lot.ManagerId,
            DeletedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Lot {LotId} deleted", lotId);
    }

    // ── Toggle Open/Closed (Manager/Admin) ────────────────────────────────────
    public async Task<LotDto> ToggleOpenAsync(int lotId, int managerId, string role)
    {
        var lot = await GetAndValidateLotAsync(lotId);

        if (role != "ADMIN" && lot.ManagerId != managerId)
            throw new UnauthorizedAccessException("You can only toggle your own lots.");

        if (lot.ApprovalStatus != "APPROVED")
            throw new InvalidOperationException(
                "Lot must be approved before it can be opened.");

        lot.IsOpen = !lot.IsOpen;
        var updated = await _repository.UpdateAsync(lot);

        await _publishEndpoint.Publish(new LotStatusChangedEvent
        {
            LotId = lotId,
            ManagerId = lot.ManagerId,
            IsOpen = lot.IsOpen,
            ChangedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Lot {LotId} toggled: IsOpen={IsOpen}", lotId, lot.IsOpen);
        return MapToDto(updated);
    }

    // ── Get Lots By Manager ───────────────────────────────────────────────────
    public async Task<List<LotDto>> GetLotsByManagerAsync(int managerId)
    {
        var lots = await _repository.FindByManagerIdAsync(managerId);
        return lots.Select(MapToDto).ToList();
    }

    // ── Approve Lot (Admin) ───────────────────────────────────────────────────
    public async Task<LotDto> ApproveLotAsync(int lotId, int adminId)
    {
        var lot = await GetAndValidateLotAsync(lotId);

        if (lot.ApprovalStatus == "APPROVED")
            throw new InvalidOperationException("Lot is already approved.");

        lot.ApprovalStatus = "APPROVED";
        lot.ApprovedAt = DateTime.UtcNow;
        lot.ApprovedByAdminId = adminId;
        lot.RejectionReason = null;

        var updated = await _repository.UpdateAsync(lot);

        // Triggers LotApprovalSaga → notifies manager
        await _publishEndpoint.Publish(new LotApprovedEvent
        {
            LotId = lotId,
            ManagerId = lot.ManagerId,
            LotName = lot.Name,
            ApprovedByAdminId = adminId,
            ApprovedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Lot {LotId} approved by Admin {AdminId}", lotId, adminId);
        return MapToDto(updated);
    }

    // ── Reject Lot (Admin) ────────────────────────────────────────────────────
    public async Task<LotDto> RejectLotAsync(int lotId, int adminId, string reason)
    {
        var lot = await GetAndValidateLotAsync(lotId);

        lot.ApprovalStatus = "REJECTED";
        lot.RejectionReason = reason;
        lot.IsOpen = false;

        var updated = await _repository.UpdateAsync(lot);

        // Triggers LotApprovalSaga → notifies manager of rejection
        await _publishEndpoint.Publish(new LotRejectedEvent
        {
            LotId = lotId,
            ManagerId = lot.ManagerId,
            LotName = lot.Name,
            Reason = reason,
            RejectedAt = DateTime.UtcNow
        });

        _logger.LogInformation(
            "Lot {LotId} rejected by Admin {AdminId}. Reason: {Reason}",
            lotId, adminId, reason);

        return MapToDto(updated);
    }

    // ── Get Pending Lots (Admin) ──────────────────────────────────────────────
    public async Task<List<LotDto>> GetPendingLotsAsync()
    {
        var lots = await _repository.FindByApprovalStatusAsync("PENDING_APPROVAL");
        return lots.Select(MapToDto).ToList();
    }

    // ── Get All Lots (Admin) ──────────────────────────────────────────────────
    public async Task<List<LotDto>> GetAllLotsAsync()
    {
        var lots = await _repository.GetAllAsync();
        return lots.Select(MapToDto).ToList();
    }

    // ── Get Lot By Id (Public) ────────────────────────────────────────────────
    public async Task<LotDto> GetLotByIdAsync(int lotId)
    {
        var lot = await GetAndValidateLotAsync(lotId);
        return MapToDto(lot);
    }

    // ── Search By City (Public) ───────────────────────────────────────────────
    public async Task<List<LotDto>> SearchLotsByCityAsync(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty.");

        var lots = await _repository.FindByCityAsync(city);
        return lots.Select(MapToDto).ToList();
    }

    // ── Nearby Lots via Haversine (Public/Driver) ─────────────────────────────
    public async Task<List<NearbyLotDto>> GetNearbyLotsAsync(
        double latitude, double longitude, double radiusKm = 5.0)
    {
        // Get all approved + open lots
        var allLots = await _repository.FindAllApprovedAndOpenAsync();

        return allLots
            .Select(lot => new
            {
                Lot = lot,
                Distance = HaversineHelper.CalculateDistance(
                    latitude, longitude,
                    lot.Latitude, lot.Longitude)
            })
            .Where(x => x.Distance <= radiusKm)
            .OrderBy(x => x.Distance)
            .Select(x => new NearbyLotDto
            {
                LotId = x.Lot.LotId,
                Name = x.Lot.Name,
                Address = x.Lot.Address,
                City = x.Lot.City,
                Latitude = x.Lot.Latitude,
                Longitude = x.Lot.Longitude,
                DistanceKm = Math.Round(x.Distance, 2),
                AvailableSpots = x.Lot.AvailableSpots,
                TotalSpots = x.Lot.TotalSpots,
                IsOpen = x.Lot.IsOpen,
                OpenTime = x.Lot.OpenTime.ToString("HH:mm"),
                CloseTime = x.Lot.CloseTime.ToString("HH:mm"),
                ImageUrl = x.Lot.ImageUrl
            })
            .ToList();
    }

    // ── Internal Spot Count Updates ───────────────────────────────────────────
    public async Task IncrementAvailableSpotsAsync(int lotId) =>
        await _repository.IncrementAvailableSpotsAsync(lotId);

    public async Task DecrementAvailableSpotsAsync(int lotId) =>
        await _repository.DecrementAvailableSpotsAsync(lotId);

    public async Task UpdateSpotCountsAsync(int lotId, int totalSpots, int availableSpots) =>
        await _repository.UpdateSpotCountsAsync(lotId, totalSpots, availableSpots);

    // ── Helpers ───────────────────────────────────────────────────────────────
    private async Task<Entities.ParkingLot> GetAndValidateLotAsync(int lotId)
    {
        return await _repository.FindByLotIdAsync(lotId)
            ?? throw new KeyNotFoundException($"Parking lot {lotId} not found.");
    }

    public static LotDto MapToDto(Entities.ParkingLot l) => new()
    {
        LotId = l.LotId,
        ManagerId = l.ManagerId,
        Name = l.Name,
        Address = l.Address,
        City = l.City,
        Latitude = l.Latitude,
        Longitude = l.Longitude,
        TotalSpots = l.TotalSpots,
        AvailableSpots = l.AvailableSpots,
        IsOpen = l.IsOpen,
        OpenTime = l.OpenTime.ToString("HH:mm"),
        CloseTime = l.CloseTime.ToString("HH:mm"),
        ImageUrl = l.ImageUrl,
        ApprovalStatus = l.ApprovalStatus,
        RejectionReason = l.RejectionReason,
        ApprovedAt = l.ApprovedAt,
        CreatedAt = l.CreatedAt
    };
}
