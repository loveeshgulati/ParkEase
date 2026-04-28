using Microsoft.EntityFrameworkCore;
using ParkEase.Auth.Data;
using ParkEase.Auth.Entities;
using ParkEase.Auth.Interfaces;

namespace ParkEase.Auth.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AuthDbContext _context;

    public AuditLogRepository(AuthDbContext context) => _context = context;

    public async Task CreateAsync(AuditLog log)
    {
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> FindByActorUserIdAsync(int actorUserId) =>
        await _context.AuditLogs
            .Where(a => a.ActorUserId == actorUserId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

    public async Task<List<AuditLog>> FindByActionAsync(string action) =>
        await _context.AuditLogs
            .Where(a => a.Action == action)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

    public async Task<List<AuditLog>> GetAllAsync() =>
        await _context.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
}
