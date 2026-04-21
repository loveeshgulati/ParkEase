using MassTransit;
using ParkEase.Auth.DTOs;
using ParkEase.Auth.Entities;
using ParkEase.Auth.Events;
using ParkEase.Auth.Interfaces;

namespace ParkEase.Auth.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        IPublishEndpoint publishEndpoint,
        ILogger<AdminService> logger)
    {
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    // ── Manager Management ────────────────────────────────────────────────────

    public async Task<List<PendingManagerDto>> GetPendingManagersAsync()
    {
        var managers = await _userRepository
            .FindAllByRoleAndStatusAsync("MANAGER", "PENDING_APPROVAL");

        return managers.Select(m => new PendingManagerDto
        {
            UserId = m.UserId,
            FullName = m.FullName,
            Email = m.Email,
            Phone = m.Phone,
            RegisteredAt = m.CreatedAt,
            Status = m.Status
        }).ToList();
    }

    public async Task<List<ManagerDto>> GetAllManagersAsync()
    {
        var managers = await _userRepository.FindAllByRoleAsync("MANAGER");

        return managers.Select(m => new ManagerDto
        {
            UserId = m.UserId,
            FullName = m.FullName,
            Email = m.Email,
            Phone = m.Phone,
            Status = m.Status,
            CreatedAt = m.CreatedAt,
            ApprovedAt = m.ApprovedAt
        }).ToList();
    }

    public async Task<UserProfileDto> GetManagerByIdAsync(int managerId)
    {
        var manager = await _userRepository.FindByUserIdAsync(managerId)
            ?? throw new KeyNotFoundException($"Manager {managerId} not found.");

        if (manager.Role != "MANAGER")
            throw new InvalidOperationException($"User {managerId} is not a manager.");

        return AuthService.MapToProfileDto(manager);
    }

    public async Task ApproveManagerAsync(int managerId, int adminId)
    {
        var manager = await _userRepository.FindByUserIdAsync(managerId)
            ?? throw new KeyNotFoundException($"Manager {managerId} not found.");

        if (manager.Role != "MANAGER")
            throw new InvalidOperationException($"User {managerId} is not a manager.");

        if (manager.Status != "PENDING_APPROVAL")
            throw new InvalidOperationException($"Manager is not in PENDING_APPROVAL status.");

        var before = System.Text.Json.JsonSerializer.Serialize(
            new { manager.Status, manager.IsActive });

        manager.Status = "ACTIVE";
        manager.IsActive = true;
        manager.ApprovedAt = DateTime.UtcNow;
        manager.ApprovedByAdminId = adminId;

        await _userRepository.UpdateAsync(manager);

        await _auditLogRepository.CreateAsync(new AuditLog
        {
            ActorUserId = adminId,
            Action = "MANAGER_APPROVED",
            TargetUserId = managerId.ToString(),
            Before = before,
            After = System.Text.Json.JsonSerializer.Serialize(
                new { Status = "ACTIVE", IsActive = true }),
            Timestamp = DateTime.UtcNow,
            Success = true
        });

        await _publishEndpoint.Publish(new ManagerApprovedEvent
        {
            ManagerId = managerId,
            Email = manager.Email,
            FullName = manager.FullName,
            ApprovedByAdminId = adminId,
            ApprovedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Manager {ManagerId} approved by Admin {AdminId}", managerId, adminId);
    }

    public async Task RejectManagerAsync(int managerId, int adminId, string reason)
    {
        var manager = await _userRepository.FindByUserIdAsync(managerId)
            ?? throw new KeyNotFoundException($"Manager {managerId} not found.");

        if (manager.Role != "MANAGER")
            throw new InvalidOperationException($"User {managerId} is not a manager.");

        var before = System.Text.Json.JsonSerializer.Serialize(new { manager.Status });

        manager.Status = "REJECTED";
        manager.IsActive = false;
        manager.RejectionReason = reason;

        await _userRepository.UpdateAsync(manager);

        await _auditLogRepository.CreateAsync(new AuditLog
        {
            ActorUserId = adminId,
            Action = "MANAGER_REJECTED",
            TargetUserId = managerId.ToString(),
            Before = before,
            After = System.Text.Json.JsonSerializer.Serialize(
                new { Status = "REJECTED", Reason = reason }),
            Timestamp = DateTime.UtcNow,
            Success = true
        });

        await _publishEndpoint.Publish(new ManagerRejectedEvent
        {
            ManagerId = managerId,
            Email = manager.Email,
            Reason = reason,
            RejectedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Manager {ManagerId} rejected by Admin {AdminId}. Reason: {Reason}",
            managerId, adminId, reason);
    }

    public async Task SuspendManagerAsync(int managerId, int adminId, string reason)
    {
        var manager = await GetAndValidateUserAsync(managerId, "MANAGER");

        var before = System.Text.Json.JsonSerializer.Serialize(new { manager.Status });

        manager.Status = "SUSPENDED";
        manager.IsActive = false;
        manager.RefreshToken = null;
        manager.RefreshTokenExpiry = null;

        await _userRepository.UpdateAsync(manager);

        await LogAuditAsync(adminId, "MANAGER_SUSPENDED", managerId,
            before, System.Text.Json.JsonSerializer.Serialize(new { Status = "SUSPENDED", Reason = reason }));

        await _publishEndpoint.Publish(new ManagerSuspendedEvent
        {
            ManagerId = managerId,
            Email = manager.Email,
            Reason = reason,
            SuspendedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Manager {ManagerId} suspended by Admin {AdminId}", managerId, adminId);
    }

    public async Task ReactivateManagerAsync(int managerId, int adminId)
    {
        var manager = await GetAndValidateUserAsync(managerId, "MANAGER");

        var before = System.Text.Json.JsonSerializer.Serialize(new { manager.Status });

        manager.Status = "ACTIVE";
        manager.IsActive = true;

        await _userRepository.UpdateAsync(manager);

        await LogAuditAsync(adminId, "MANAGER_REACTIVATED", managerId,
            before, System.Text.Json.JsonSerializer.Serialize(new { Status = "ACTIVE" }));

        await _publishEndpoint.Publish(new ManagerReactivatedEvent
        {
            ManagerId = managerId,
            Email = manager.Email,
            ReactivatedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Manager {ManagerId} reactivated by Admin {AdminId}", managerId, adminId);
    }

    public async Task DeleteManagerAsync(int managerId, int adminId)
    {
        var manager = await GetAndValidateUserAsync(managerId, "MANAGER");

        await _userRepository.DeleteByUserIdAsync(managerId);

        await LogAuditAsync(adminId, "MANAGER_DELETED", managerId,
            System.Text.Json.JsonSerializer.Serialize(new { manager.Email, manager.Status }), null);

        await _publishEndpoint.Publish(new ManagerDeletedEvent
        {
            ManagerId = managerId,
            Email = manager.Email,
            DeletedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Manager {ManagerId} deleted by Admin {AdminId}", managerId, adminId);
    }

    // ── Driver Management ─────────────────────────────────────────────────────

    public async Task<List<DriverDto>> GetAllDriversAsync()
    {
        var drivers = await _userRepository.FindAllByRoleAsync("DRIVER");

        return drivers.Select(d => new DriverDto
        {
            UserId = d.UserId,
            FullName = d.FullName,
            Email = d.Email,
            Phone = d.Phone,
            Status = d.Status,
            VehiclePlate = d.VehiclePlate,
            CreatedAt = d.CreatedAt
        }).ToList();
    }

    public async Task<UserProfileDto> GetDriverByIdAsync(int driverId)
    {
        var driver = await _userRepository.FindByUserIdAsync(driverId)
            ?? throw new KeyNotFoundException($"Driver {driverId} not found.");

        if (driver.Role != "DRIVER")
            throw new InvalidOperationException($"User {driverId} is not a driver.");

        return AuthService.MapToProfileDto(driver);
    }

    public async Task SuspendDriverAsync(int driverId, int adminId, string reason)
    {
        var driver = await GetAndValidateUserAsync(driverId, "DRIVER");

        var before = System.Text.Json.JsonSerializer.Serialize(new { driver.Status });

        driver.Status = "SUSPENDED";
        driver.IsActive = false;
        driver.RefreshToken = null;
        driver.RefreshTokenExpiry = null;

        await _userRepository.UpdateAsync(driver);

        await LogAuditAsync(adminId, "DRIVER_SUSPENDED", driverId,
            before, System.Text.Json.JsonSerializer.Serialize(new { Status = "SUSPENDED", Reason = reason }));

        await _publishEndpoint.Publish(new DriverSuspendedEvent
        {
            DriverId = driverId,
            Email = driver.Email,
            Reason = reason,
            SuspendedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Driver {DriverId} suspended by Admin {AdminId}", driverId, adminId);
    }

    public async Task ReactivateDriverAsync(int driverId, int adminId)
    {
        var driver = await GetAndValidateUserAsync(driverId, "DRIVER");

        var before = System.Text.Json.JsonSerializer.Serialize(new { driver.Status });

        driver.Status = "ACTIVE";
        driver.IsActive = true;

        await _userRepository.UpdateAsync(driver);

        await LogAuditAsync(adminId, "DRIVER_REACTIVATED", driverId,
            before, System.Text.Json.JsonSerializer.Serialize(new { Status = "ACTIVE" }));

        await _publishEndpoint.Publish(new DriverReactivatedEvent
        {
            DriverId = driverId,
            Email = driver.Email,
            ReactivatedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Driver {DriverId} reactivated by Admin {AdminId}", driverId, adminId);
    }

    public async Task DeleteDriverAsync(int driverId, int adminId)
    {
        var driver = await GetAndValidateUserAsync(driverId, "DRIVER");

        await _userRepository.DeleteByUserIdAsync(driverId);

        await LogAuditAsync(adminId, "DRIVER_DELETED", driverId,
            System.Text.Json.JsonSerializer.Serialize(new { driver.Email, driver.Status }), null);

        await _publishEndpoint.Publish(new DriverDeletedEvent
        {
            DriverId = driverId,
            Email = driver.Email,
            DeletedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Driver {DriverId} deleted by Admin {AdminId}", driverId, adminId);
    }

    // ── Platform Overview ─────────────────────────────────────────────────────

    public async Task<List<UserProfileDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(AuthService.MapToProfileDto).ToList();
    }

    public async Task<UserProfileDto> GetUserByIdAsync(int userId)
    {
        var user = await _userRepository.FindByUserIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");
        return AuthService.MapToProfileDto(user);
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private async Task<Entities.User> GetAndValidateUserAsync(int userId, string expectedRole)
    {
        var user = await _userRepository.FindByUserIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        if (user.Role != expectedRole)
            throw new InvalidOperationException($"User {userId} is not a {expectedRole}.");

        return user;
    }

    private async Task LogAuditAsync(int actorId, string action, int targetId,
        string? before, string? after)
    {
        await _auditLogRepository.CreateAsync(new AuditLog
        {
            ActorUserId = actorId,
            Action = action,
            TargetUserId = targetId.ToString(),
            Before = before,
            After = after,
            Timestamp = DateTime.UtcNow,
            Success = true
        });
    }
}
