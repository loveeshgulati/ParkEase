using ParkEase.Auth.DTOs;

namespace ParkEase.Auth.Interfaces;

public interface IAdminService
{
    // ── Manager Management ────────────────────────────────────────────────────
    Task<List<PendingManagerDto>> GetPendingManagersAsync();
    Task<List<ManagerDto>> GetAllManagersAsync();
    Task<UserProfileDto> GetManagerByIdAsync(int managerId);
    Task ApproveManagerAsync(int managerId, int adminId);
    Task RejectManagerAsync(int managerId, int adminId, string reason);
    Task SuspendManagerAsync(int managerId, int adminId, string reason);
    Task ReactivateManagerAsync(int managerId, int adminId);
    Task DeleteManagerAsync(int managerId, int adminId);

    // ── Driver Management ─────────────────────────────────────────────────────
    Task<List<DriverDto>> GetAllDriversAsync();
    Task<UserProfileDto> GetDriverByIdAsync(int driverId);
    Task SuspendDriverAsync(int driverId, int adminId, string reason);
    Task ReactivateDriverAsync(int driverId, int adminId);
    Task DeleteDriverAsync(int driverId, int adminId);

    // ── Platform Overview ─────────────────────────────────────────────────────
    Task<List<UserProfileDto>> GetAllUsersAsync();
    Task<UserProfileDto> GetUserByIdAsync(int userId);
}
