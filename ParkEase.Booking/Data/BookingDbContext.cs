using Microsoft.EntityFrameworkCore;
using BookingEntity = ParkEase.Booking.Entities.Booking;

namespace ParkEase.Booking.Data;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options)
        : base(options) { }

    public DbSet<BookingEntity> Bookings => Set<BookingEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BookingEntity>(entity =>
        {
            entity.ToTable("bookings");
            entity.HasKey(b => b.BookingId);
            entity.Property(b => b.BookingId).HasColumnName("booking_id").UseIdentityColumn();
            entity.Property(b => b.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(b => b.LotId).HasColumnName("lot_id").IsRequired();
            entity.Property(b => b.SpotId).HasColumnName("spot_id").IsRequired();
            entity.Property(b => b.VehiclePlate).HasColumnName("vehicle_plate")
                .HasMaxLength(20).IsRequired();
            entity.Property(b => b.VehicleType).HasColumnName("vehicle_type")
                .HasMaxLength(10).IsRequired();
            entity.Property(b => b.BookingType).HasColumnName("booking_type")
                .HasMaxLength(20).IsRequired();
            entity.Property(b => b.Status).HasColumnName("status")
                .HasMaxLength(20).HasDefaultValue("RESERVED");
            entity.Property(b => b.StartTime).HasColumnName("start_time").IsRequired();
            entity.Property(b => b.EndTime).HasColumnName("end_time").IsRequired();
            entity.Property(b => b.CheckInTime).HasColumnName("check_in_time");
            entity.Property(b => b.CheckOutTime).HasColumnName("check_out_time");
            entity.Property(b => b.TotalAmount).HasColumnName("total_amount")
                .HasDefaultValue(0);
            entity.Property(b => b.CancellationReason).HasColumnName("cancellation_reason");
            entity.Property(b => b.CreatedAt).HasColumnName("created_at");
            entity.Property(b => b.UpdatedAt).HasColumnName("updated_at");

            // Indexes for fast queries
            entity.HasIndex(b => b.UserId);
            entity.HasIndex(b => b.LotId);
            entity.HasIndex(b => b.SpotId);
            entity.HasIndex(b => b.Status);
            entity.HasIndex(b => new { b.SpotId, b.Status });
        });
    }
}
