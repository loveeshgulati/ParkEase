using ParkEase.Payment.DTOs;
using PaymentEntity = ParkEase.Payment.Entities.Payment;

namespace ParkEase.Payment.Interfaces;

public interface IPaymentRepository
{
    Task<PaymentEntity?> FindByPaymentIdAsync(int paymentId);
    Task<PaymentEntity?> FindByBookingIdAsync(int bookingId);
    Task<List<PaymentEntity>> FindByUserIdAsync(int userId);
    Task<List<PaymentEntity>> FindByStatusAsync(string status);
    Task<List<PaymentEntity>> GetAllAsync();
    Task<double> SumAmountByUserIdAsync(int userId);
    Task<PaymentEntity> CreateAsync(PaymentEntity payment);
    Task<PaymentEntity> UpdateAsync(PaymentEntity payment);
}

public interface IPaymentService
{
    // ── Razorpay order creation ───────────────────────────────────────────────
    Task<RazorpayOrderResponseDto> CreateRazorpayOrderAsync(CreateRazorpayOrderDto request);

    // ── Driver actions ────────────────────────────────────────────────────────
    Task<PaymentDto> ProcessPaymentAsync(int userId, ProcessPaymentDto request);
    Task<PaymentDto> GetPaymentByIdAsync(int paymentId, int userId, string role);
    Task<PaymentDto> GetPaymentByBookingIdAsync(int bookingId, int userId, string role);
    Task<List<PaymentDto>> GetMyPaymentsAsync(int userId);
    Task<PaymentDto> RefundPaymentAsync(int userId, string role, RefundPaymentDto request);
    Task<string> GenerateReceiptAsync(int paymentId, int userId, string role);

    // ── Manager actions ───────────────────────────────────────────────────────
    Task<RevenueDto> GetRevenueByLotAsync(int lotId, DateTime from, DateTime to);

    // ── Admin actions ─────────────────────────────────────────────────────────
    Task<List<PaymentDto>> GetAllPaymentsAsync();
    Task<PlatformRevenueDto> GetPlatformRevenueAsync(DateTime from, DateTime to);
}
