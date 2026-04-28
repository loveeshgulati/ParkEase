using ParkEase.Auth.Entities;

namespace ParkEase.Auth.Interfaces;

public interface IAuditLogRepository
{
    Task CreateAsync(AuditLog log);
    Task<List<AuditLog>> FindByActorUserIdAsync(int actorUserId);
    Task<List<AuditLog>> FindByActionAsync(string action);
    Task<List<AuditLog>> GetAllAsync();
}
