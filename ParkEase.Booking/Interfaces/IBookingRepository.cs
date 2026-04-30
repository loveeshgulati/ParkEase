using BookingEntity = ParkEase.Booking.Entities.Booking;

namespace ParkEase.Booking.Interfaces;

public interface IBookingRepository
{
    Task<BookingEntity?> FindByBookingIdAsync(int bookingId);
    Task<List<BookingEntity>> FindByUserIdAsync(int userId);
    Task<List<BookingEntity>> FindByLotIdAsync(int lotId);
    Task<List<BookingEntity>> FindBySpotIdAsync(int spotId);
    Task<List<BookingEntity>> FindByStatusAsync(string status);
    Task<List<BookingEntity>> FindByUserIdAndStatusAsync(int userId, string status);
    Task<List<BookingEntity>> FindByLotIdAndStatusAsync(int lotId, string status);
    Task<BookingEntity?> FindActiveBySpotIdAsync(int spotId);
    Task<List<BookingEntity>> FindExpiredPreBookingsAsync(DateTime graceDeadline);
    Task<List<BookingEntity>> GetAllAsync();
    Task<BookingEntity> CreateAsync(BookingEntity booking);
    Task<BookingEntity> UpdateAsync(BookingEntity booking);
}
