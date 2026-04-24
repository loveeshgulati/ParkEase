using Microsoft.EntityFrameworkCore;
using PaymentEntity = ParkEase.Payment.Entities.Payment;

namespace ParkEase.Payment.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options) { }

    public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PaymentEntity>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(p => p.PaymentId);
            entity.Property(p => p.PaymentId).HasColumnName("payment_id").UseIdentityColumn();
            entity.Property(p => p.BookingId).HasColumnName("booking_id").IsRequired();
            entity.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(p => p.Amount).HasColumnName("amount").IsRequired();
            entity.Property(p => p.Status).HasColumnName("status")
                .HasMaxLength(20).HasDefaultValue("PENDING");
            entity.Property(p => p.Mode).HasColumnName("mode").HasMaxLength(20).IsRequired();

            // Razorpay fields (replaces transaction_id)
            entity.Property(p => p.RazorpayOrderId).HasColumnName("razorpay_order_id");
            entity.Property(p => p.RazorpayPaymentId).HasColumnName("razorpay_payment_id");

            entity.Property(p => p.Currency).HasColumnName("currency")
                .HasMaxLength(10).HasDefaultValue("INR");
            entity.Property(p => p.Description).HasColumnName("description");
            entity.Property(p => p.PaidAt).HasColumnName("paid_at");
            entity.Property(p => p.RefundedAt).HasColumnName("refunded_at");
            entity.Property(p => p.RefundAmount).HasColumnName("refund_amount");
            entity.Property(p => p.RefundReason).HasColumnName("refund_reason");
            entity.Property(p => p.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(p => p.BookingId);
            entity.HasIndex(p => p.UserId);
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.RazorpayOrderId);
            entity.HasIndex(p => p.RazorpayPaymentId);
        });
    }
}
