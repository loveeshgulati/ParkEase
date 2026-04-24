using MassTransit;
using ParkEase.Payment.DTOs;
using PaymentEntity = ParkEase.Payment.Entities.Payment;
using ParkEase.Payment.Events;
using ParkEase.Payment.Interfaces;

namespace ParkEase.Payment.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly IRazorpayService _razorpayService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentService> _logger;

    private static readonly string[] ValidModes = { "CARD", "UPI", "WALLET", "CASH" };

    public PaymentService(
        IPaymentRepository repository,
        IRazorpayService razorpayService,
        IPublishEndpoint publishEndpoint,
        IConfiguration configuration,
        ILogger<PaymentService> logger)
    {
        _repository      = repository;
        _razorpayService = razorpayService;
        _publishEndpoint = publishEndpoint;
        _configuration   = configuration;
        _logger          = logger;
    }

    // ── Create Razorpay Order ─────────────────────────────────────────────────

    public async Task<RazorpayOrderResponseDto> CreateRazorpayOrderAsync(
        CreateRazorpayOrderDto request)
    {
        _logger.LogInformation(
            "Creating Razorpay order for ₹{Amount}", request.Amount);

        var orderId = await _razorpayService.CreateOrderAsync(
            request.Amount, request.Receipt);

        return new RazorpayOrderResponseDto
        {
            OrderId  = orderId,
            Key      = _configuration["Razorpay:KeyId"]!,
            Amount   = request.Amount,
            Currency = "INR"
        };
    }

    // ── Process Payment (with Razorpay signature verification) ───────────────

    public async Task<PaymentDto> ProcessPaymentAsync(
        int userId, ProcessPaymentDto request)
    {
        var mode = request.Mode.ToUpper();
        if (!ValidModes.Contains(mode))
            throw new InvalidOperationException(
                $"Invalid payment mode. Must be: {string.Join(", ", ValidModes)}");

        // ── Step 1: Verify Razorpay signature ─────────────────────────────────
        _logger.LogInformation(
            "Verifying Razorpay signature for OrderId={OrderId} PaymentId={PaymentId}",
            request.RazorpayOrderId, request.RazorpayPaymentId);

        var isSignatureValid = _razorpayService.VerifySignature(
            request.RazorpayOrderId,
            request.RazorpayPaymentId,
            request.RazorpaySignature);

        if (!isSignatureValid)
        {
            _logger.LogWarning(
                "Payment verification FAILED for BookingId={BookingId}. Invalid signature.",
                request.BookingId);

            // Persist a FAILED record so audit trail is intact
            var failedPayment = new PaymentEntity
            {
                BookingId        = request.BookingId,
                UserId           = userId,
                Amount           = request.Amount,
                Mode             = mode,
                RazorpayOrderId  = request.RazorpayOrderId,
                RazorpayPaymentId = request.RazorpayPaymentId,
                Status           = "FAILED",
                Currency         = "INR",
                Description      = "Payment failed: invalid Razorpay signature",
                CreatedAt        = DateTime.UtcNow
            };
            var saved = await _repository.CreateAsync(failedPayment);

            await _publishEndpoint.Publish(new PaymentFailedEvent
            {
                BookingId  = saved.BookingId,
                UserId     = saved.UserId,
                Amount     = saved.Amount,
                Reason     = "Invalid Razorpay signature",
                FailedAt   = DateTime.UtcNow
            });

            throw new InvalidOperationException(
                "Payment verification failed: Razorpay signature is invalid.");
        }

        // ── Step 2: Check for existing payment for this booking ────────────────
        var existing = await _repository.FindByBookingIdAsync(request.BookingId);

        PaymentEntity payment;

        if (existing != null)
        {
            if (existing.Status == "PAID")
                throw new InvalidOperationException(
                    "Payment already processed for this booking.");

            // Update existing PENDING/FAILED record with new Razorpay details
            existing.Mode              = mode;
            existing.RazorpayOrderId   = request.RazorpayOrderId;
            existing.RazorpayPaymentId = request.RazorpayPaymentId;
            existing.Status            = "PAID";
            existing.PaidAt            = DateTime.UtcNow;
            existing.Description       = request.Description ?? existing.Description;
            payment = await _repository.UpdateAsync(existing);
        }
        else
        {
            // Create new payment record
            payment = new PaymentEntity
            {
                BookingId         = request.BookingId,
                UserId            = userId,
                Amount            = request.Amount,
                Mode              = mode,
                RazorpayOrderId   = request.RazorpayOrderId,
                RazorpayPaymentId = request.RazorpayPaymentId,
                Status            = "PAID",
                Currency          = "INR",
                Description       = request.Description
                    ?? $"Parking fee for Booking #{request.BookingId}",
                PaidAt    = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            payment = await _repository.CreateAsync(payment);
        }

        // ── Step 3: Publish success event ─────────────────────────────────────
        await _publishEndpoint.Publish(new PaymentProcessedEvent
        {
            PaymentId         = payment.PaymentId,
            BookingId         = payment.BookingId,
            UserId            = payment.UserId,
            Amount            = payment.Amount,
            Mode              = payment.Mode,
            TransactionId     = payment.RazorpayPaymentId,   // keep event contract compatible
            PaidAt            = payment.PaidAt!.Value
        });

        _logger.LogInformation(
            "Payment {PaymentId} verified and processed. Amount=₹{Amount} Mode={Mode} " +
            "RazorpayPaymentId={RazorpayPaymentId}",
            payment.PaymentId, payment.Amount, payment.Mode, payment.RazorpayPaymentId);

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

        payment.Status       = "REFUNDED";
        payment.RefundAmount = refundAmt;
        payment.RefundReason = request.Reason;
        payment.RefundedAt   = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(payment);

        await _publishEndpoint.Publish(new RefundProcessedEvent
        {
            PaymentId    = payment.PaymentId,
            BookingId    = payment.BookingId,
            UserId       = payment.UserId,
            RefundAmount = refundAmt,
            Reason       = request.Reason,
            RefundedAt   = payment.RefundedAt!.Value
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

        var receipt = $"""
            ==========================================
            ParkEase — Payment Receipt
            ==========================================
            Razorpay Payment ID : {payment.RazorpayPaymentId ?? "N/A"}
            Razorpay Order ID   : {payment.RazorpayOrderId   ?? "N/A"}
            Payment ID          : {payment.PaymentId}
            Booking ID          : {payment.BookingId}
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
        var allPayments = await _repository.GetAllAsync();
        var lotPayments = allPayments
            .Where(p => p.Status == "PAID"
                && p.PaidAt >= from
                && p.PaidAt <= to)
            .ToList();

        return new RevenueDto
        {
            LotId            = lotId,
            TotalRevenue     = lotPayments.Sum(p => p.Amount),
            TotalTransactions = lotPayments.Count,
            FromDate         = from,
            ToDate           = to
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
            TotalRevenue      = paid.Sum(p => p.Amount),
            TotalTransactions = paid.Count,
            FromDate          = from,
            ToDate            = to
        };
    }

    // ── Get All Payments (Admin) ──────────────────────────────────────────────
    public async Task<List<PaymentDto>> GetAllPaymentsAsync()
    {
        var payments = await _repository.GetAllAsync();
        return payments.Select(MapToDto).ToList();
    }

    // ── Mapper ────────────────────────────────────────────────────────────────
    public static PaymentDto MapToDto(PaymentEntity p) => new()
    {
        PaymentId         = p.PaymentId,
        BookingId         = p.BookingId,
        UserId            = p.UserId,
        Amount            = p.Amount,
        Status            = p.Status,
        Mode              = p.Mode,
        RazorpayOrderId   = p.RazorpayOrderId,
        RazorpayPaymentId = p.RazorpayPaymentId,
        Currency          = p.Currency,
        Description       = p.Description,
        PaidAt            = p.PaidAt,
        RefundedAt        = p.RefundedAt,
        RefundAmount      = p.RefundAmount,
        RefundReason      = p.RefundReason,
        CreatedAt         = p.CreatedAt
    };
}
