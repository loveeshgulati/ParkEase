using MassTransit;
using ParkEase.Booking.DTOs.Request;
using ParkEase.Booking.DTOs.Response;
using ParkEase.Booking.DTOs.Common;
using ParkEase.Booking.Events.Published;
using ParkEase.Booking.Interfaces;
using BookingEntity = ParkEase.Booking.Entities.Booking;

namespace ParkEase.Booking.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _repository;
    private readonly ISpotHttpClient _spotHttpClient;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<BookingService> _logger;

    // Grace period for pre-bookings: 30 minutes after start time
    private const int GracePeriodMinutes = 30;

    // Minimum booking charge: 1 hour
    private const double MinimumHours = 1.0;

    public BookingService(
        IBookingRepository repository,
        ISpotHttpClient spotHttpClient,
        IPublishEndpoint publishEndpoint,
        ILogger<BookingService> logger)
    {
        _repository = repository;
        _spotHttpClient = spotHttpClient;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    // ── Create Booking ────────────────────────────────────────────────────────
    public async Task<BookingDto> CreateBookingAsync(int userId, CreateBookingDto request)
    {
        // Validate times
        if (request.StartTime >= request.EndTime)
            throw new InvalidOperationException("End time must be after start time.");

        if (request.StartTime < DateTime.UtcNow.AddMinutes(-5))
            throw new InvalidOperationException("Start time cannot be in the past.");

        // Get spot info from spot-service
        var spotInfo = await _spotHttpClient.GetSpotAsync(request.SpotId)
            ?? throw new InvalidOperationException(
                $"Spot {request.SpotId} not found or unavailable.");

        if (spotInfo.Status != "AVAILABLE")
            throw new InvalidOperationException(
                $"Spot {request.SpotId} is not available. Status: {spotInfo.Status}");

        // Check for existing active booking on this spot
        var existingBooking = await _repository.FindActiveBySpotIdAsync(request.SpotId);
        if (existingBooking != null)
            throw new InvalidOperationException(
                "This spot already has an active booking.");

        var bookingType = request.BookingType.ToUpper();
        if (bookingType != "PRE_BOOKING" && bookingType != "WALK_IN")
            throw new InvalidOperationException(
                "BookingType must be PRE_BOOKING or WALK_IN.");

        // Reserve spot via spot-service HTTP call
        var reserved = await _spotHttpClient.ReserveSpotAsync(request.SpotId);
        if (!reserved)
            throw new InvalidOperationException(
                "Failed to reserve spot. Please try again.");

        var booking = new BookingEntity
        {
            UserId = userId,
            LotId = request.LotId,
            SpotId = request.SpotId,
            VehiclePlate = request.VehiclePlate.ToUpper(),
            VehicleType = request.VehicleType.ToUpper(),
            BookingType = bookingType,
            Status = "RESERVED",
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            TotalAmount = 0,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(booking);

        // Publish event for notification-service
        await _publishEndpoint.Publish(new BookingCreatedEvent
        {
            BookingId = created.BookingId,
            UserId = created.UserId,
            LotId = created.LotId,
            SpotId = created.SpotId,
            VehiclePlate = created.VehiclePlate,
            BookingType = created.BookingType,
            StartTime = created.StartTime,
            EndTime = created.EndTime,
            CreatedAt = created.CreatedAt
        });

        _logger.LogInformation(
            "Booking {BookingId} created for User={UserId} Spot={SpotId}",
            created.BookingId, userId, request.SpotId);

        return MapToDto(created);
    }

    // ── Cancel Booking ────────────────────────────────────────────────────────
    public async Task<BookingDto> CancelBookingAsync(
        int bookingId, int userId, string role, string? reason)
    {
        var booking = await GetAndValidateBookingAsync(bookingId);

        // RBAC: driver can only cancel own bookings
        if (role == "DRIVER" && booking.UserId != userId)
            throw new UnauthorizedAccessException(
                "You can only cancel your own bookings.");

        if (booking.Status == "CANCELLED" || booking.Status == "COMPLETED")
            throw new InvalidOperationException(
                $"Cannot cancel a booking with status: {booking.Status}");

        // Determine refund eligibility
        var isEligibleForRefund = booking.Status == "RESERVED" &&
            booking.BookingType == "PRE_BOOKING";

        booking.Status = "CANCELLED";
        booking.CancellationReason = reason ?? "Cancelled by user";
        var updated = await _repository.UpdateAsync(booking);

        // Release spot back to AVAILABLE
        await _spotHttpClient.ReleaseSpotAsync(booking.SpotId);

        await _publishEndpoint.Publish(new BookingCancelledEvent
        {
            BookingId = bookingId,
            UserId = booking.UserId,
            LotId = booking.LotId,
            SpotId = booking.SpotId,
            Reason = booking.CancellationReason,
            IsEligibleForRefund = isEligibleForRefund,
            RefundAmount = isEligibleForRefund ? booking.TotalAmount : 0,
            CancelledAt = DateTime.UtcNow
        });

        _logger.LogInformation("Booking {BookingId} cancelled", bookingId);
        return MapToDto(updated);
    }

    // ── Check In ──────────────────────────────────────────────────────────────
    public async Task<BookingDto> CheckInAsync(int bookingId, int userId, string role)
    {
        var booking = await GetAndValidateBookingAsync(bookingId);

        if (role == "DRIVER" && booking.UserId != userId)
            throw new UnauthorizedAccessException(
                "You can only check in to your own booking.");

        if (booking.Status != "RESERVED")
            throw new InvalidOperationException(
                $"Booking must be RESERVED to check in. Current: {booking.Status}");

        booking.Status = "ACTIVE";
        booking.CheckInTime = DateTime.UtcNow;
        var updated = await _repository.UpdateAsync(booking);

        // Transition spot: RESERVED → OCCUPIED
        await _spotHttpClient.OccupySpotAsync(booking.SpotId);

        await _publishEndpoint.Publish(new BookingCheckedInEvent
        {
            BookingId = bookingId,
            UserId = booking.UserId,
            LotId = booking.LotId,
            SpotId = booking.SpotId,
            CheckInTime = booking.CheckInTime!.Value
        });

        _logger.LogInformation(
            "Booking {BookingId} checked in at {CheckIn}", bookingId, booking.CheckInTime);
        return MapToDto(updated);
    }

    // ── Check Out ─────────────────────────────────────────────────────────────
    public async Task<BookingDto> CheckOutAsync(int bookingId, int userId, string role)
    {
        var booking = await GetAndValidateBookingAsync(bookingId);

        if (role == "DRIVER" && booking.UserId != userId)
            throw new UnauthorizedAccessException(
                "You can only check out of your own booking.");

        if (booking.Status != "ACTIVE")
            throw new InvalidOperationException(
                $"Booking must be ACTIVE to check out. Current: {booking.Status}");

        booking.CheckOutTime = DateTime.UtcNow;
        booking.Status = "COMPLETED";

        // Calculate fare
        var fare = await CalculateFareInternalAsync(booking);
        booking.TotalAmount = fare;

        var updated = await _repository.UpdateAsync(booking);

        // Release spot: OCCUPIED → AVAILABLE
        await _spotHttpClient.ReleaseSpotAsync(booking.SpotId);

        await _publishEndpoint.Publish(new BookingCheckedOutEvent
        {
            BookingId = bookingId,
            UserId = booking.UserId,
            LotId = booking.LotId,
            SpotId = booking.SpotId,
            CheckOutTime = booking.CheckOutTime!.Value,
            TotalAmount = booking.TotalAmount
        });

        _logger.LogInformation(
            "Booking {BookingId} checked out. Amount=₹{Amount}",
            bookingId, booking.TotalAmount);
        return MapToDto(updated);
    }

    // ── Extend Booking ────────────────────────────────────────────────────────
    public async Task<BookingDto> ExtendBookingAsync(
        int bookingId, int userId, ExtendBookingDto request)
    {
        var booking = await GetAndValidateBookingAsync(bookingId);

        if (booking.UserId != userId)
            throw new UnauthorizedAccessException(
                "You can only extend your own bookings.");

        if (booking.Status != "RESERVED" && booking.Status != "ACTIVE")
            throw new InvalidOperationException(
                "Can only extend RESERVED or ACTIVE bookings.");

        if (request.NewEndTime <= booking.EndTime)
            throw new InvalidOperationException(
                "New end time must be after current end time.");

        // Check spot is still available for extension
        var spotInfo = await _spotHttpClient.GetSpotAsync(booking.SpotId);
        if (spotInfo == null)
            throw new InvalidOperationException("Spot not found.");

        booking.EndTime = request.NewEndTime;
        var updated = await _repository.UpdateAsync(booking);

        await _publishEndpoint.Publish(new BookingExtendedEvent
        {
            BookingId = bookingId,
            UserId = userId,
            NewEndTime = request.NewEndTime,
            ExtendedAt = DateTime.UtcNow
        });

        _logger.LogInformation(
            "Booking {BookingId} extended to {NewEnd}", bookingId, request.NewEndTime);
        return MapToDto(updated);
    }

    // ── Get My Bookings ───────────────────────────────────────────────────────
    public async Task<List<BookingDto>> GetMyBookingsAsync(int userId)
    {
        var bookings = await _repository.FindByUserIdAsync(userId);
        return bookings.Select(MapToDto).ToList();
    }

    // ── Get Booking By Id ─────────────────────────────────────────────────────
    public async Task<BookingDto> GetBookingByIdAsync(
        int bookingId, int userId, string role)
    {
        var booking = await GetAndValidateBookingAsync(bookingId);

        if (role == "DRIVER" && booking.UserId != userId)
            throw new UnauthorizedAccessException(
                "You can only view your own bookings.");

        return MapToDto(booking);
    }

    // ── Calculate Fare ────────────────────────────────────────────────────────
    public async Task<FareCalculationDto> CalculateFareAsync(int bookingId)
    {
        var booking = await GetAndValidateBookingAsync(bookingId);
        var spotInfo = await _spotHttpClient.GetSpotAsync(booking.SpotId);

        if (spotInfo == null)
            throw new InvalidOperationException("Spot info unavailable.");

        var checkOut = booking.CheckOutTime ?? DateTime.UtcNow;
        var checkIn = booking.CheckInTime ?? booking.StartTime;
        var hours = Math.Max((checkOut - checkIn).TotalHours, MinimumHours);
        var total = Math.Round(hours * spotInfo.PricePerHour, 2);

        return new FareCalculationDto
        {
            BookingId = bookingId,
            SpotId = booking.SpotId,
            PricePerHour = spotInfo.PricePerHour,
            HoursParked = Math.Round(hours, 2),
            TotalAmount = total,
            Note = hours < 1 ? "Minimum 1 hour charge applied" : string.Empty
        };
    }

    // ── Get Bookings By Lot (Manager) ─────────────────────────────────────────
    public async Task<List<BookingDto>> GetBookingsByLotAsync(
        int lotId, int managerId, string role)
    {
        var bookings = await _repository.FindByLotIdAsync(lotId);
        return bookings.Select(MapToDto).ToList();
    }

    // ── Get Active Bookings By Lot (Manager) ──────────────────────────────────
    public async Task<List<BookingDto>> GetActiveBookingsByLotAsync(
        int lotId, int managerId, string role)
    {
        var bookings = await _repository.FindByLotIdAndStatusAsync(lotId, "ACTIVE");
        return bookings.Select(MapToDto).ToList();
    }

    // ── Force Checkout (Manager) ──────────────────────────────────────────────
    public async Task<BookingDto> ForceCheckOutAsync(int bookingId, int managerId)
    {
        var booking = await GetAndValidateBookingAsync(bookingId);

        if (booking.Status != "ACTIVE")
            throw new InvalidOperationException(
                "Can only force checkout on ACTIVE bookings.");

        booking.CheckOutTime = DateTime.UtcNow;
        booking.Status = "COMPLETED";
        booking.TotalAmount = await CalculateFareInternalAsync(booking);
        booking.CancellationReason = "Force checkout by manager";
        var updated = await _repository.UpdateAsync(booking);

        await _spotHttpClient.ReleaseSpotAsync(booking.SpotId);

        await _publishEndpoint.Publish(new BookingCheckedOutEvent
        {
            BookingId = bookingId,
            UserId = booking.UserId,
            LotId = booking.LotId,
            SpotId = booking.SpotId,
            CheckOutTime = booking.CheckOutTime!.Value,
            TotalAmount = booking.TotalAmount
        });

        _logger.LogInformation(
            "Booking {BookingId} force checked out by Manager {ManagerId}",
            bookingId, managerId);
        return MapToDto(updated);
    }

    // ── Get All Bookings (Admin) ──────────────────────────────────────────────
    public async Task<List<BookingDto>> GetAllBookingsAsync()
    {
        var bookings = await _repository.GetAllAsync();
        return bookings.Select(MapToDto).ToList();
    }

    // ── Auto Cancel Expired Bookings (Background Service) ─────────────────────
    public async Task AutoCancelExpiredBookingsAsync()
    {
        var graceDeadline = DateTime.UtcNow.AddMinutes(-GracePeriodMinutes);
        var expired = await _repository.FindExpiredPreBookingsAsync(graceDeadline);

        foreach (var booking in expired)
        {
            booking.Status = "EXPIRED";
            booking.CancellationReason = "Auto-cancelled: check-in grace period elapsed";
            await _repository.UpdateAsync(booking);

            await _spotHttpClient.ReleaseSpotAsync(booking.SpotId);

            await _publishEndpoint.Publish(new BookingExpiredEvent
            {
                BookingId = booking.BookingId,
                UserId = booking.UserId,
                LotId = booking.LotId,
                SpotId = booking.SpotId,
                ExpiredAt = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Booking {BookingId} auto-expired", booking.BookingId);
        }

        if (expired.Any())
            _logger.LogInformation(
                "{Count} expired bookings auto-cancelled", expired.Count);
    }

    // ── Private Helpers ───────────────────────────────────────────────────────
    private async Task<BookingEntity> GetAndValidateBookingAsync(int bookingId) =>
        await _repository.FindByBookingIdAsync(bookingId)
            ?? throw new KeyNotFoundException($"Booking {bookingId} not found.");

    private async Task<double> CalculateFareInternalAsync(BookingEntity booking)
    {
        var spotInfo = await _spotHttpClient.GetSpotAsync(booking.SpotId);
        if (spotInfo == null) return 0;

        var checkOut = booking.CheckOutTime ?? DateTime.UtcNow;
        var checkIn = booking.CheckInTime ?? booking.StartTime;
        var hours = Math.Max((checkOut - checkIn).TotalHours, MinimumHours);
        return Math.Round(hours * spotInfo.PricePerHour, 2);
    }

    public static BookingDto MapToDto(BookingEntity b) => new()
    {
        BookingId = b.BookingId,
        UserId = b.UserId,
        LotId = b.LotId,
        SpotId = b.SpotId,
        VehiclePlate = b.VehiclePlate,
        VehicleType = b.VehicleType,
        BookingType = b.BookingType,
        Status = b.Status,
        StartTime = b.StartTime,
        EndTime = b.EndTime,
        CheckInTime = b.CheckInTime,
        CheckOutTime = b.CheckOutTime,
        TotalAmount = b.TotalAmount,
        CancellationReason = b.CancellationReason,
        CreatedAt = b.CreatedAt
    };
}
