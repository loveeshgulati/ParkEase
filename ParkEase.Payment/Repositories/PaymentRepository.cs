using Microsoft.EntityFrameworkCore;
using ParkEase.Payment.Data;
using PaymentEntity = ParkEase.Payment.Entities.Payment;
using ParkEase.Payment.Interfaces;

namespace ParkEase.Payment.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    public PaymentRepository(PaymentDbContext context) => _context = context;

    public async Task<PaymentEntity?> FindByPaymentIdAsync(int paymentId) =>
        await _context.Payments.FindAsync(paymentId);

    public async Task<PaymentEntity?> FindByBookingIdAsync(int bookingId) =>
        await _context.Payments
            .FirstOrDefaultAsync(p => p.BookingId == bookingId);

    public async Task<List<PaymentEntity>> FindByUserIdAsync(int userId) =>
        await _context.Payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<List<PaymentEntity>> FindByStatusAsync(string status) =>
        await _context.Payments
            .Where(p => p.Status == status)
            .ToListAsync();

    public async Task<List<PaymentEntity>> GetAllAsync() =>
        await _context.Payments
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<double> SumAmountByUserIdAsync(int userId) =>
        await _context.Payments
            .Where(p => p.UserId == userId && p.Status == "PAID")
            .SumAsync(p => p.Amount);

    public async Task<PaymentEntity> CreateAsync(PaymentEntity payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<PaymentEntity> UpdateAsync(PaymentEntity payment)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
        return payment;
    }
}
