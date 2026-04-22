using Microsoft.EntityFrameworkCore;
using ParkEase.ParkingLot.Data;
using ParkEase.ParkingLot.Interfaces;

namespace ParkEase.ParkingLot.Repositories;

public class ParkingLotRepository : IParkingLotRepository
{
    private readonly ParkingLotDbContext _context;

    public ParkingLotRepository(ParkingLotDbContext context) => _context = context;

    public async Task<Entities.ParkingLot?> FindByLotIdAsync(int lotId) =>
        await _context.ParkingLots.FindAsync(lotId);

    public async Task<List<Entities.ParkingLot>> FindByCityAsync(string city) =>
        await _context.ParkingLots
            .Where(l => l.City.ToLower() == city.ToLower()
                && l.ApprovalStatus == "APPROVED")
            .ToListAsync();

    public async Task<List<Entities.ParkingLot>> FindByManagerIdAsync(int managerId) =>
        await _context.ParkingLots
            .Where(l => l.ManagerId == managerId)
            .ToListAsync();

    public async Task<List<Entities.ParkingLot>> FindByApprovalStatusAsync(string status) =>
        await _context.ParkingLots
            .Where(l => l.ApprovalStatus == status)
            .ToListAsync();

    public async Task<List<Entities.ParkingLot>> FindAllApprovedAndOpenAsync() =>
        await _context.ParkingLots
            .Where(l => l.ApprovalStatus == "APPROVED" && l.IsOpen)
            .ToListAsync();

    public async Task<List<Entities.ParkingLot>> GetAllAsync() =>
        await _context.ParkingLots.ToListAsync();

    public async Task<Entities.ParkingLot> CreateAsync(Entities.ParkingLot lot)
    {
        _context.ParkingLots.Add(lot);
        await _context.SaveChangesAsync();
        return lot;
    }

    public async Task<Entities.ParkingLot> UpdateAsync(Entities.ParkingLot lot)
    {
        lot.UpdatedAt = DateTime.UtcNow;
        _context.ParkingLots.Update(lot);
        await _context.SaveChangesAsync();
        return lot;
    }

    public async Task DeleteByLotIdAsync(int lotId)
    {
        var lot = await _context.ParkingLots.FindAsync(lotId);
        if (lot != null)
        {
            _context.ParkingLots.Remove(lot);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAllByManagerIdAsync(int managerId)
    {
        var lots = await _context.ParkingLots
            .Where(l => l.ManagerId == managerId)
            .ToListAsync();

        if (lots.Any())
        {
            _context.ParkingLots.RemoveRange(lots);
            await _context.SaveChangesAsync();
        }
    }

    public async Task IncrementAvailableSpotsAsync(int lotId)
    {
        var lot = await _context.ParkingLots.FindAsync(lotId);
        if (lot != null)
        {
            lot.AvailableSpots = Math.Min(lot.AvailableSpots + 1, lot.TotalSpots);
            lot.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DecrementAvailableSpotsAsync(int lotId)
    {
        var lot = await _context.ParkingLots.FindAsync(lotId);
        if (lot != null)
        {
            lot.AvailableSpots = Math.Max(lot.AvailableSpots - 1, 0);
            lot.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateSpotCountsAsync(int lotId, int totalSpots, int availableSpots)
    {
        var lot = await _context.ParkingLots.FindAsync(lotId);
        if (lot != null)
        {
            lot.TotalSpots = totalSpots;
            lot.AvailableSpots = availableSpots;
            lot.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
