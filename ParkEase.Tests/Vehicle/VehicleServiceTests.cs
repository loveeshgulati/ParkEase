using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using ParkEase.Vehicle.DTOs;
using ParkEase.Vehicle.Interfaces;
using ParkEase.Vehicle.Services;

namespace ParkEase.Tests.Vehicle;

[TestFixture]
public class VehicleServiceTests
{
    private Mock<IVehicleRepository> _repo = null!;
    private Mock<IPublishEndpoint>   _bus  = null!;
    private VehicleService           _sut  = null!;

    [SetUp]
    public void SetUp()
    {
        _repo = new Mock<IVehicleRepository>();
        _bus  = new Mock<IPublishEndpoint>();
        _bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new VehicleService(
            _repo.Object, _bus.Object, NullLogger<VehicleService>.Instance);
    }

    // ── Register ──────────────────────────────────────────────────────────────

    [Test]
    public async Task RegisterVehicle_ValidRequest_ReturnsVehicleDto()
    {
        _repo.Setup(r => r.ExistsByLicensePlateAndOwnerAsync("MH01AB1234", 1)).ReturnsAsync(false);
        _repo.Setup(r => r.CreateAsync(It.IsAny<ParkEase.Vehicle.Entities.Vehicle>()))
            .ReturnsAsync((ParkEase.Vehicle.Entities.Vehicle v) => { v.VehicleId = 10; return v; });

        var result = await _sut.RegisterVehicleAsync(ownerId: 1, new RegisterVehicleDto
        {
            LicensePlate = "MH01AB1234",
            Make = "Toyota", Model = "Corolla",
            Color = "White", VehicleType = "4W", IsEV = false
        });

        Assert.That(result.VehicleId,    Is.EqualTo(10));
        Assert.That(result.LicensePlate, Is.EqualTo("MH01AB1234"));
    }

    [Test]
    public void RegisterVehicle_InvalidVehicleType_ThrowsInvalidOperation()
    {
        _repo.Setup(r => r.ExistsByLicensePlateAndOwnerAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(false);

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.RegisterVehicleAsync(1, new RegisterVehicleDto
            {
                LicensePlate = "XX00YY0000", Make = "X", Model = "Y",
                Color = "Red", VehicleType = "TRUCK", IsEV = false
            }));
    }

    // ── Get Vehicle ───────────────────────────────────────────────────────────

    [Test]
    public async Task GetVehicleById_AdminRole_CanViewAnyVehicle()
    {
        var vehicle = new ParkEase.Vehicle.Entities.Vehicle
        {
            VehicleId = 5, OwnerId = 99, LicensePlate = "KA01XX1111",
            Make = "Honda", Model = "City", VehicleType = "4W"
        };
        _repo.Setup(r => r.FindByVehicleIdAsync(5)).ReturnsAsync(vehicle);

        var result = await _sut.GetVehicleByIdAsync(5,
            requestingUserId: 1, requestingUserRole: "ADMIN");

        Assert.That(result.VehicleId, Is.EqualTo(5));
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Test]
    public async Task DeleteVehicle_OwnerDeletes_CallsRepository()
    {
        var vehicle = new ParkEase.Vehicle.Entities.Vehicle { VehicleId = 7, OwnerId = 5 };
        _repo.Setup(r => r.FindByVehicleIdAsync(7)).ReturnsAsync(vehicle);
        _repo.Setup(r => r.DeleteByVehicleIdAsync(7)).Returns(Task.CompletedTask);

        await _sut.DeleteVehicleAsync(7, ownerId: 5, role: "DRIVER");

        _repo.Verify(r => r.DeleteByVehicleIdAsync(7), Times.Once);
    }
}
