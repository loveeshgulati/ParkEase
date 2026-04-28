using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using ParkEase.Payment.DTOs;
using ParkEase.Payment.Entities;
using ParkEase.Payment.Interfaces;
using ParkEase.Payment.Services;

namespace ParkEase.Tests.Payment;

[TestFixture]
public class PaymentServiceTests
{
    private Mock<IPaymentRepository> _repo     = null!;
    private Mock<IPublishEndpoint>   _bus      = null!;
    private Mock<IRazorpayService>   _razorpay = null!;
    private PaymentService           _sut      = null!;

    [SetUp]
    public void SetUp()
    {
        _repo     = new Mock<IPaymentRepository>();
        _bus      = new Mock<IPublishEndpoint>();
        _razorpay = new Mock<IRazorpayService>();
        _bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _sut = new PaymentService(
            _repo.Object, _bus.Object,
            NullLogger<PaymentService>.Instance, _razorpay.Object);
    }

    [Test]
    public async Task ProcessPayment_ValidUPIMode_ReturnsPaymentDto()
    {
        _repo.Setup(r => r.FindByBookingIdAsync(1))
            .ReturnsAsync((ParkEase.Payment.Entities.Payment?)null);
        _repo.Setup(r => r.CreateAsync(It.IsAny<ParkEase.Payment.Entities.Payment>()))
            .ReturnsAsync((ParkEase.Payment.Entities.Payment p) => { p.PaymentId = 10; return p; });

        var result = await _sut.ProcessPaymentAsync(userId: 5, new ProcessPaymentDto
        {
            BookingId = 1, Amount = 150.0, Mode = "UPI", TransactionId = "TXN123"
        });

        Assert.That(result.Status, Is.EqualTo("PAID"));
        Assert.That(result.Mode,   Is.EqualTo("UPI"));
    }

    [Test]
    public void ProcessPayment_InvalidMode_ThrowsInvalidOperation()
    {
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.ProcessPaymentAsync(1, new ProcessPaymentDto
            {
                BookingId = 2, Amount = 100.0, Mode = "BITCOIN"
            }));
    }

    [Test]
    public async Task RefundPayment_PaidPayment_SetsRefunded()
    {
        var payment = new ParkEase.Payment.Entities.Payment
        {
            PaymentId = 20, BookingId = 10, UserId = 5, Status = "PAID", Amount = 300.0
        };
        _repo.Setup(r => r.FindByPaymentIdAsync(20)).ReturnsAsync(payment);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<ParkEase.Payment.Entities.Payment>()))
            .ReturnsAsync(payment);

        var result = await _sut.RefundPaymentAsync(userId: 5, role: "DRIVER",
            new RefundPaymentDto { PaymentId = 20, Reason = "Cancelled" });

        Assert.That(result.Status, Is.EqualTo("REFUNDED"));
    }

    [Test]
    public void RefundPayment_OtherUsersPayment_ThrowsUnauthorized()
    {
        var payment = new ParkEase.Payment.Entities.Payment
        {
            PaymentId = 22, UserId = 99, Status = "PAID"
        };
        _repo.Setup(r => r.FindByPaymentIdAsync(22)).ReturnsAsync(payment);

        Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.RefundPaymentAsync(userId: 5, role: "DRIVER",
                new RefundPaymentDto { PaymentId = 22 }));
    }
}
