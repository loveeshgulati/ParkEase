using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using ParkEase.Booking.DTOs.Request;
using ParkEase.Booking.Interfaces;
using ParkEase.Booking.Services;
using BookingEntity = ParkEase.Booking.Entities.Booking;

namespace ParkEase.Tests.Booking;

[TestFixture]
public class BookingServiceTests
{
    private Mock<IBookingRepository> _repo       = null!;
    private Mock<ISpotHttpClient>    _spotClient = null!;
    private Mock<IPublishEndpoint>   _bus        = null!;
    private BookingService           _sut        = null!;

    private SpotInfo _availableSpot = null!;

    [SetUp]
    public void SetUp()
    {
        _repo       = new Mock<IBookingRepository>();
        _spotClient = new Mock<ISpotHttpClient>();
        _bus        = new Mock<IPublishEndpoint>();

        _availableSpot = new SpotInfo
        {
            SpotId = 1, LotId = 1,
            Status = "AVAILABLE", PricePerHour = 50.0,
            SpotType = "STANDARD", VehicleType = "4W"
        };

        _bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new BookingService(
            _repo.Object, _spotClient.Object,
            _bus.Object, NullLogger<BookingService>.Instance);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Test]
    public async Task CreateBooking_ValidRequest_ReturnsReservedBooking()
    {
        var start = DateTime.UtcNow.AddMinutes(5);
        var end   = start.AddHours(2);

        _spotClient.Setup(c => c.GetSpotAsync(1)).ReturnsAsync(_availableSpot);
        _spotClient.Setup(c => c.ReserveSpotAsync(1)).ReturnsAsync(true);
        _repo.Setup(r => r.FindActiveBySpotIdAsync(1)).ReturnsAsync((BookingEntity?)null);
        _repo.Setup(r => r.CreateAsync(It.IsAny<BookingEntity>()))
            .ReturnsAsync((BookingEntity b) => { b.BookingId = 101; return b; });

        var result = await _sut.CreateBookingAsync(userId: 5, new CreateBookingDto
        {
            SpotId = 1, LotId = 1, VehiclePlate = "DL01AB1234",
            VehicleType = "4W", BookingType = "PRE_BOOKING",
            StartTime = start, EndTime = end
        });

        Assert.That(result.Status,    Is.EqualTo("RESERVED"));
        Assert.That(result.BookingId, Is.EqualTo(101));
    }

    [Test]
    public void CreateBooking_EndBeforeStart_ThrowsInvalidOperation()
    {
        var now = DateTime.UtcNow;
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateBookingAsync(1, new CreateBookingDto
            {
                SpotId = 1, LotId = 1, VehiclePlate = "X",
                VehicleType = "4W", BookingType = "PRE_BOOKING",
                StartTime = now.AddHours(2), EndTime = now.AddHours(1)
            }));
    }

    // ── Cancel ────────────────────────────────────────────────────────────────

    [Test]
    public async Task CancelBooking_OwnReservedBooking_SetsCancelled()
    {
        var booking = new BookingEntity
        {
            BookingId = 1, UserId = 5, SpotId = 1,
            Status = "RESERVED", BookingType = "PRE_BOOKING"
        };
        _repo.Setup(r => r.FindByBookingIdAsync(1)).ReturnsAsync(booking);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<BookingEntity>())).ReturnsAsync(booking);
        _spotClient.Setup(c => c.ReleaseSpotAsync(1)).ReturnsAsync(true);

        var result = await _sut.CancelBookingAsync(1, userId: 5, role: "DRIVER", reason: "Change of plans");

        Assert.That(result.Status, Is.EqualTo("CANCELLED"));
    }

    // ── Check In ──────────────────────────────────────────────────────────────

    [Test]
    public async Task CheckIn_ReservedBooking_SetsActive()
    {
        var booking = new BookingEntity { BookingId = 10, UserId = 5, SpotId = 1, Status = "RESERVED" };
        _repo.Setup(r => r.FindByBookingIdAsync(10)).ReturnsAsync(booking);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<BookingEntity>())).ReturnsAsync(booking);
        _spotClient.Setup(c => c.OccupySpotAsync(1)).ReturnsAsync(true);

        var result = await _sut.CheckInAsync(10, userId: 5, role: "DRIVER");

        Assert.That(result.Status, Is.EqualTo("ACTIVE"));
    }

    // ── Check Out ─────────────────────────────────────────────────────────────

    [Test]
    public async Task CheckOut_ActiveBooking_SetsCompleted()
    {
        var booking = new BookingEntity
        {
            BookingId = 20, UserId = 5, SpotId = 1, Status = "ACTIVE",
            CheckInTime = DateTime.UtcNow.AddHours(-2),
            StartTime   = DateTime.UtcNow.AddHours(-2),
            EndTime     = DateTime.UtcNow.AddHours(1)
        };
        _repo.Setup(r => r.FindByBookingIdAsync(20)).ReturnsAsync(booking);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<BookingEntity>())).ReturnsAsync(booking);
        _spotClient.Setup(c => c.GetSpotAsync(1)).ReturnsAsync(_availableSpot);
        _spotClient.Setup(c => c.ReleaseSpotAsync(1)).ReturnsAsync(true);

        var result = await _sut.CheckOutAsync(20, userId: 5, role: "DRIVER");

        Assert.That(result.Status,      Is.EqualTo("COMPLETED"));
        Assert.That(result.TotalAmount, Is.GreaterThan(0));
    }
}
