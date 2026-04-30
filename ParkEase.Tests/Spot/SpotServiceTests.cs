using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using ParkEase.Spot.DTOs.Request;
using ParkEase.Spot.Entities;
using ParkEase.Spot.Interfaces;
using ParkEase.Spot.Services;

namespace ParkEase.Tests.Spot;

[TestFixture]
public class SpotServiceTests
{
    private Mock<ISpotRepository>  _repo = null!;
    private Mock<IPublishEndpoint> _bus  = null!;
    private SpotService            _sut  = null!;

    private static ParkingSpot MakeSpot(int spotId, int lotId, string status = "AVAILABLE") =>
        new()
        {
            SpotId = spotId, LotId = lotId,
            SpotNumber = $"A-{spotId:D2}", Floor = 0,
            SpotType = "STANDARD", VehicleType = "4W",
            Status = status, PricePerHour = 40.0,
            CreatedAt = DateTime.UtcNow
        };

    [SetUp]
    public void SetUp()
    {
        _repo = new Mock<ISpotRepository>();
        _bus  = new Mock<IPublishEndpoint>();
        _bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repo.Setup(r => r.CountByLotIdAsync(It.IsAny<int>())).ReturnsAsync(5);
        _repo.Setup(r => r.CountByLotIdAndStatusAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(3);
        _sut = new SpotService(_repo.Object, _bus.Object, NullLogger<SpotService>.Instance);
    }

    [Test]
    public async Task AddSpot_ValidRequest_ReturnsAvailableSpot()
    {
        var spot = MakeSpot(1, 10);
        _repo.Setup(r => r.ExistsBySpotNumberAndLotAsync("A-01", 10)).ReturnsAsync(false);
        _repo.Setup(r => r.CreateAsync(It.IsAny<ParkingSpot>())).ReturnsAsync(spot);

        var result = await _sut.AddSpotAsync(managerId: 1, new AddSpotDto
        {
            LotId = 10, SpotNumber = "A-01", Floor = 0,
            SpotType = "STANDARD", VehicleType = "4W",
            PricePerHour = 40.0, IsHandicapped = false, IsEVCharging = false
        });

        Assert.That(result.Status, Is.EqualTo("AVAILABLE"));
    }

    [Test]
    public void AddSpot_InvalidSpotType_ThrowsInvalidOperation()
    {
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.AddSpotAsync(1, new AddSpotDto
            {
                LotId = 10, SpotNumber = "Z-01", Floor = 0,
                SpotType = "INVALID", VehicleType = "4W", PricePerHour = 40.0
            }));
    }

    [Test]
    public async Task ReserveSpot_AvailableSpot_SetsReserved()
    {
        var spot = MakeSpot(1, 10, "AVAILABLE");
        _repo.Setup(r => r.FindBySpotIdAsync(1)).ReturnsAsync(spot);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<ParkingSpot>())).ReturnsAsync(spot);

        var result = await _sut.ReserveSpotAsync(1);

        Assert.That(result.Status, Is.EqualTo("RESERVED"));
    }

    [Test]
    public async Task OccupySpot_ReservedSpot_SetsOccupied()
    {
        var spot = MakeSpot(3, 10, "RESERVED");
        _repo.Setup(r => r.FindBySpotIdAsync(3)).ReturnsAsync(spot);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<ParkingSpot>())).ReturnsAsync(spot);

        var result = await _sut.OccupySpotAsync(3);

        Assert.That(result.Status, Is.EqualTo("OCCUPIED"));
    }

    [Test]
    public async Task ReleaseSpot_OccupiedSpot_SetsAvailable()
    {
        var spot = MakeSpot(5, 10, "OCCUPIED");
        _repo.Setup(r => r.FindBySpotIdAsync(5)).ReturnsAsync(spot);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<ParkingSpot>())).ReturnsAsync(spot);

        var result = await _sut.ReleaseSpotAsync(5);

        Assert.That(result.Status, Is.EqualTo("AVAILABLE"));
    }
}
