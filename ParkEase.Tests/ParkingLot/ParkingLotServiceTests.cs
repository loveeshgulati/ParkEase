using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using ParkEase.ParkingLot.DTOs.Request;
using ParkEase.ParkingLot.Interfaces;
using ParkEase.ParkingLot.Services;

namespace ParkEase.Tests.ParkingLot;

[TestFixture]
public class ParkingLotServiceTests
{
    private Mock<IParkingLotRepository> _repo = null!;
    private Mock<IPublishEndpoint>      _bus  = null!;
    private ParkingLotService           _sut  = null!;

    private static ParkEase.ParkingLot.Entities.ParkingLot MakeLot(
        int lotId, int managerId, string approval = "PENDING_APPROVAL", bool isOpen = false) =>
        new()
        {
            LotId = lotId, ManagerId = managerId, Name = "Test Lot",
            Address = "123 Main St", City = "TestCity",
            Latitude = 28.6, Longitude = 77.2,
            OpenTime  = new TimeOnly(8, 0),
            CloseTime = new TimeOnly(22, 0),
            ApprovalStatus = approval,
            IsOpen = isOpen,
            CreatedAt = DateTime.UtcNow
        };

    [SetUp]
    public void SetUp()
    {
        _repo = new Mock<IParkingLotRepository>();
        _bus  = new Mock<IPublishEndpoint>();
        _bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new ParkingLotService(
            _repo.Object, _bus.Object, NullLogger<ParkingLotService>.Instance);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Test]
    public async Task CreateLot_ValidRequest_ReturnsPendingApproval()
    {
        var lot = MakeLot(1, 10);
        _repo.Setup(r => r.CreateAsync(It.IsAny<ParkEase.ParkingLot.Entities.ParkingLot>()))
            .ReturnsAsync(lot);

        var result = await _sut.CreateLotAsync(managerId: 10, new CreateLotDto
        {
            Name = "Test Lot", Address = "123 Main St", City = "TestCity",
            Latitude = 28.6, Longitude = 77.2,
            OpenTime = "08:00", CloseTime = "22:00"
        });

        Assert.That(result.ApprovalStatus, Is.EqualTo("PENDING_APPROVAL"));
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Test]
    public void UpdateLot_DifferentManager_ThrowsUnauthorized()
    {
        var lot = MakeLot(1, managerId: 10);
        _repo.Setup(r => r.FindByLotIdAsync(1)).ReturnsAsync(lot);

        Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.UpdateLotAsync(1, managerId: 99, new UpdateLotDto { Name = "X" }));
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Test]
    public async Task DeleteLot_AdminRole_DeletesAnyLot()
    {
        var lot = MakeLot(2, managerId: 5);
        _repo.Setup(r => r.FindByLotIdAsync(2)).ReturnsAsync(lot);
        _repo.Setup(r => r.DeleteByLotIdAsync(2)).Returns(Task.CompletedTask);

        await _sut.DeleteLotAsync(2, managerId: 99, role: "ADMIN");

        _repo.Verify(r => r.DeleteByLotIdAsync(2), Times.Once);
    }

    // ── Approve ───────────────────────────────────────────────────────────────

    [Test]
    public async Task ApproveLot_PendingLot_SetsApproved()
    {
        var lot = MakeLot(5, 10, approval: "PENDING_APPROVAL");
        _repo.Setup(r => r.FindByLotIdAsync(5)).ReturnsAsync(lot);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<ParkEase.ParkingLot.Entities.ParkingLot>()))
            .ReturnsAsync(lot);

        var result = await _sut.ApproveLotAsync(5, adminId: 1);

        Assert.That(result.ApprovalStatus, Is.EqualTo("APPROVED"));
    }

    // ── Toggle Open ───────────────────────────────────────────────────────────

    [Test]
    public async Task ToggleOpen_ApprovedLot_TogglesIsOpen()
    {
        var lot = MakeLot(8, 10, approval: "APPROVED", isOpen: false);
        _repo.Setup(r => r.FindByLotIdAsync(8)).ReturnsAsync(lot);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<ParkEase.ParkingLot.Entities.ParkingLot>()))
            .ReturnsAsync(lot);

        var result = await _sut.ToggleOpenAsync(8, managerId: 10, role: "MANAGER");

        Assert.That(result.IsOpen, Is.True);
    }
}
