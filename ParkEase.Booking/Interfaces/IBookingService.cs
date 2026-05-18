using ParkEase.Booking.DTOs.Request;
using ParkEase.Booking.DTOs.Response;

namespace ParkEase.Booking.Interfaces;

public interface IBookingService
{
    
    Task<BookingDto> CreateBookingAsync(int userId, CreateBookingDto request);
    Task<BookingDto> CancelBookingAsync(int bookingId, int userId, string role, string? reason);
    Task<BookingDto> CheckInAsync(int bookingId, int userId, string role);
    Task<BookingDto> CheckOutAsync(int bookingId, int userId, string role);
    Task<BookingDto> ExtendBookingAsync(int bookingId, int userId, ExtendBookingDto request);
    Task<List<BookingDto>> GetMyBookingsAsync(int userId);
    Task<BookingDto> GetBookingByIdAsync(int bookingId, int userId, string role);
    Task<FareCalculationDto> CalculateFareAsync(int bookingId);

    
    Task<List<BookingDto>> GetBookingsByLotAsync(int lotId, int managerId, string role);
    Task<List<BookingDto>> GetActiveBookingsByLotAsync(int lotId, int managerId, string role);
    Task<BookingDto> ForceCheckOutAsync(int bookingId, int managerId);

    
    Task<List<BookingDto>> GetAllBookingsAsync();

    
    Task AutoCancelExpiredBookingsAsync();
}
