using Microsoft.EntityFrameworkCore;
using ParkEase.Booking.Data;
using ParkEase.Booking.Interfaces;
using BookingEntity = ParkEase.Booking.Entities.Booking;

namespace ParkEase.Booking.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _context;

    public BookingRepository(BookingDbContext context) => _context = context;

    public async Task<BookingEntity?> FindByBookingIdAsync(int bookingId) =>
        await _context.Bookings.FindAsync(bookingId);

    public async Task<List<BookingEntity>> FindByUserIdAsync(int userId) =>
        await _context.Bookings
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

    public async Task<List<BookingEntity>> FindByLotIdAsync(int lotId) =>
        await _context.Bookings
            .Where(b => b.LotId == lotId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

    public async Task<List<BookingEntity>> FindBySpotIdAsync(int spotId) =>
        await _context.Bookings
            .Where(b => b.SpotId == spotId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

    public async Task<List<BookingEntity>> FindByStatusAsync(string status) =>
        await _context.Bookings
            .Where(b => b.Status == status)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

    public async Task<List<BookingEntity>> FindByUserIdAndStatusAsync(int userId, string status) =>
        await _context.Bookings
            .Where(b => b.UserId == userId && b.Status == status)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

    public async Task<List<BookingEntity>> FindByLotIdAndStatusAsync(int lotId, string status) =>
        await _context.Bookings
            .Where(b => b.LotId == lotId && b.Status == status)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

    public async Task<BookingEntity?> FindActiveBySpotIdAsync(int spotId) =>
        await _context.Bookings
            .FirstOrDefaultAsync(b => b.SpotId == spotId &&
                (b.Status == "RESERVED" || b.Status == "ACTIVE"));

    public async Task<List<BookingEntity>> FindExpiredPreBookingsAsync(DateTime graceDeadline) =>
        await _context.Bookings
            .Where(b => b.Status == "RESERVED" &&
                b.BookingType == "PRE_BOOKING" &&
                b.StartTime < graceDeadline)
            .ToListAsync();

    public async Task<List<BookingEntity>> GetAllAsync() =>
        await _context.Bookings
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

    public async Task<BookingEntity> CreateAsync(BookingEntity booking)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task<BookingEntity> UpdateAsync(BookingEntity booking)
    {
        booking.UpdatedAt = DateTime.UtcNow;
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
        return booking;
    }
}
