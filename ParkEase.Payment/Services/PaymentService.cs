using MassTransit;
using ParkEase.Payment.DTOs;
using PaymentEntity = ParkEase.Payment.Entities.Payment;
using ParkEase.Payment.Events;
using ParkEase.Payment.Interfaces;

namespace ParkEase.Payment.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PaymentService> _logger;
    private readonly IRazorpayService _razorpayService;

    private static readonly string[] ValidModes = { "CARD", "UPI", "WALLET", "CASH" };

    public PaymentService(
        IPaymentRepository repository,
        IPublishEndpoint publishEndpoint,
        ILogger<PaymentService> logger,
        IRazorpayService razorpayService)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _razorpayService = razorpayService;
    }

    // ── Create Razorpay Order ───────────────────────────────────────────────────
    public async Task<RazorpayOrderDto> CreateOrderAsync(CreateOrderDto request)
    {
        var receipt = $"order_{DateTime.UtcNow:yyyyMMddHHmmss}_{Random.Shared.Next(1000, 9999)}";
        var order = await _razorpayService.CreateOrderAsync(request.Amount, receipt);
        
        return new RazorpayOrderDto
        {
            Id = order.Id,
            Entity = order.Entity,
            Amount = order.Amount,
            Currency = order.Currency,
            Receipt = order.Receipt,
            Status = order.Status,
            Attempts = order.Attempts,
            CreatedAt = order.CreatedAt
        };
    }

    // ── Process Payment ───────────────────────────────────────────────────────
    public async Task<PaymentDto> ProcessPaymentAsync(
        int userId, ProcessPaymentDto request)
    {
        var mode = request.Mode.ToUpper();
        if (!ValidModes.Contains(mode))
            throw new InvalidOperationException(
                $"Invalid payment mode. Must be: {string.Join(", ", ValidModes)}");

        // Verify Razorpay payment signature
        if (!string.IsNullOrEmpty(request.RazorpayOrderId) && 
            !string.IsNullOrEmpty(request.RazorpayPaymentId) && 
            !string.IsNullOrEmpty(request.RazorpaySignature))
        {
            var isValid = await _razorpayService.VerifyPaymentAsync(
                request.RazorpayOrderId, 
                request.RazorpayPaymentId, 
                request.RazorpaySignature);
            
            if (!isValid)
                throw new InvalidOperationException("Invalid payment signature. Payment verification failed.");
        }

        // Check if payment already exists for this booking
        var existing = await _repository.FindByBookingIdAsync(request.BookingId);

        PaymentEntity payment;

        if (existing != null)
        {
            // Update existing PENDING record
            if (existing.Status == "PAID")
                throw new InvalidOperationException(
                    "Payment already processed for this booking.");

            existing.Mode = mode;
            existing.TransactionId = request.TransactionId
                ?? GenerateTransactionId();
            existing.Status = "PAID";
            existing.PaidAt = DateTime.UtcNow;
            existing.Description = request.Description ?? existing.Description;
            payment = await _repository.UpdateAsync(existing);
        }
        else
        {
            // Create new payment record
            payment = new PaymentEntity
            {
                BookingId = request.BookingId,
                UserId = userId,
                Amount = request.Amount,
                Mode = mode,
                TransactionId = request.TransactionId ?? GenerateTransactionId(),
                Status = "PAID",
                Currency = "INR",
                Description = request.Description
                    ?? $"Parking fee for Booking #{request.BookingId}",
                PaidAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            payment = await _repository.CreateAsync(payment);
        }

        await _publishEndpoint.Publish(new PaymentProcessedEvent
        {
            PaymentId = payment.PaymentId,
            BookingId = payment.BookingId,
            UserId = payment.UserId,
            Amount = payment.Amount,
            Mode = payment.Mode,
            TransactionId = payment.TransactionId,
            PaidAt = payment.PaidAt!.Value
        });

        _logger.LogInformation(
            "Payment {PaymentId} processed. Amount=₹{Amount} Mode={Mode}",
            payment.PaymentId, payment.Amount, payment.Mode);

        return MapToDto(payment);
    }

    // ── Get Payment By Id ─────────────────────────────────────────────────────
    public async Task<PaymentDto> GetPaymentByIdAsync(
        int paymentId, int userId, string role)
    {
        var payment = await _repository.FindByPaymentIdAsync(paymentId)
            ?? throw new KeyNotFoundException($"Payment {paymentId} not found.");

        if (role == "DRIVER" && payment.UserId != userId)
            throw new UnauthorizedAccessException(
                "You can only view your own payments.");

        return MapToDto(payment);
    }

    // ── Get Payment By Booking ────────────────────────────────────────────────
    public async Task<PaymentDto> GetPaymentByBookingIdAsync(
        int bookingId, int userId, string role)
    {
        var payment = await _repository.FindByBookingIdAsync(bookingId)
            ?? throw new KeyNotFoundException(
                $"Payment for booking {bookingId} not found.");

        if (role == "DRIVER" && payment.UserId != userId)
            throw new UnauthorizedAccessException(
                "You can only view your own payments.");

        return MapToDto(payment);
    }

    // ── Get My Payments ───────────────────────────────────────────────────────
    public async Task<List<PaymentDto>> GetMyPaymentsAsync(int userId)
    {
        var payments = await _repository.FindByUserIdAsync(userId);
        return payments.Select(MapToDto).ToList();
    }

    // ── Refund Payment ────────────────────────────────────────────────────────
    public async Task<PaymentDto> RefundPaymentAsync(
        int userId, string role, RefundPaymentDto request)
    {
        var payment = await _repository.FindByPaymentIdAsync(request.PaymentId)
            ?? throw new KeyNotFoundException(
                $"Payment {request.PaymentId} not found.");

        if (role == "DRIVER" && payment.UserId != userId)
            throw new UnauthorizedAccessException(
                "You can only refund your own payments.");

        if (payment.Status != "PAID")
            throw new InvalidOperationException(
                $"Only PAID payments can be refunded. Current: {payment.Status}");

        var refundAmt = request.RefundAmount ?? payment.Amount;

        payment.Status = "REFUNDED";
        payment.RefundAmount = refundAmt;
        payment.RefundReason = request.Reason;
        payment.RefundedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(payment);

        await _publishEndpoint.Publish(new RefundProcessedEvent
        {
            PaymentId = payment.PaymentId,
            BookingId = payment.BookingId,
            UserId = payment.UserId,
            RefundAmount = refundAmt,
            Reason = request.Reason,
            RefundedAt = payment.RefundedAt!.Value
        });

        _logger.LogInformation(
            "Refund ₹{Amount} processed for Payment {PaymentId}",
            refundAmt, payment.PaymentId);

        return MapToDto(updated);
    }

    // ── Generate Receipt ──────────────────────────────────────────────────────
    public async Task<string> GenerateReceiptAsync(
        int paymentId, int userId, string role)
    {
        var payment = await _repository.FindByPaymentIdAsync(paymentId)
            ?? throw new KeyNotFoundException($"Payment {paymentId} not found.");

        if (role == "DRIVER" && payment.UserId != userId)
            throw new UnauthorizedAccessException(
                "You can only view your own receipts.");

        // Receipt as formatted text (PDF generation can be added later with QuestPDF)
        var receipt = $"""
            ==========================================
            ParkEase — Payment Receipt
            ==========================================
            Receipt No  : {payment.TransactionId}
            Payment ID  : {payment.PaymentId}
            Booking ID  : {payment.BookingId}
            ──────────────────────────────────────────
            Amount Paid : ₹{payment.Amount}
            Mode        : {payment.Mode}
            Status      : {payment.Status}
            Currency    : {payment.Currency}
            ──────────────────────────────────────────
            Paid At     : {payment.PaidAt?.ToString("dd MMM yyyy HH:mm") ?? "N/A"}
            ==========================================
            Thank you for using ParkEase!
            ==========================================
            """;

        return receipt;
    }

    // ── Get Revenue By Lot (Manager) ──────────────────────────────────────────
    public async Task<RevenueDto> GetRevenueByLotAsync(
        int lotId, DateTime from, DateTime to)
    {
        // Revenue aggregation via LINQ over payments
        // In production: join with bookings table which has lotId
        var allPayments = await _repository.GetAllAsync();
        var lotPayments = allPayments
            .Where(p => p.Status == "PAID"
                && p.PaidAt >= from
                && p.PaidAt <= to)
            .ToList();

        return new RevenueDto
        {
            LotId = lotId,
            TotalRevenue = lotPayments.Sum(p => p.Amount),
            TotalTransactions = lotPayments.Count,
            FromDate = from,
            ToDate = to
        };
    }

    // ── Get Platform Revenue (Admin) ──────────────────────────────────────────
    public async Task<PlatformRevenueDto> GetPlatformRevenueAsync(
        DateTime from, DateTime to)
    {
        var allPayments = await _repository.GetAllAsync();
        var paid = allPayments
            .Where(p => p.Status == "PAID"
                && p.PaidAt >= from
                && p.PaidAt <= to)
            .ToList();

        return new PlatformRevenueDto
        {
            TotalRevenue = paid.Sum(p => p.Amount),
            TotalTransactions = paid.Count,
            FromDate = from,
            ToDate = to
        };
    }

    // ── Get All Payments (Admin) ──────────────────────────────────────────────
    public async Task<List<PaymentDto>> GetAllPaymentsAsync()
    {
        var payments = await _repository.GetAllAsync();
        return payments.Select(MapToDto).ToList();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static string GenerateTransactionId() =>
        $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

    public static PaymentDto MapToDto(PaymentEntity p) => new()
    {
        PaymentId = p.PaymentId,
        BookingId = p.BookingId,
        UserId = p.UserId,
        Amount = p.Amount,
        Status = p.Status,
        Mode = p.Mode,
        TransactionId = p.TransactionId,
        Currency = p.Currency,
        Description = p.Description,
        PaidAt = p.PaidAt,
        RefundedAt = p.RefundedAt,
        RefundAmount = p.RefundAmount,
        RefundReason = p.RefundReason,
        CreatedAt = p.CreatedAt
    };
}
