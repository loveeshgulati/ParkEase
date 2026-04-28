using Microsoft.EntityFrameworkCore;
using ParkEase.Spot.Entities;

namespace ParkEase.Spot.Data;

public class SpotDbContext : DbContext
{
    public SpotDbContext(DbContextOptions<SpotDbContext> options) : base(options) { }

    public DbSet<ParkingSpot> ParkingSpots => Set<ParkingSpot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ParkingSpot>(entity =>
        {
            entity.ToTable("parking_spots");
            entity.HasKey(s => s.SpotId);
            entity.Property(s => s.SpotId).HasColumnName("spot_id").UseIdentityColumn();
            entity.Property(s => s.LotId).HasColumnName("lot_id").IsRequired();
            entity.Property(s => s.SpotNumber).HasColumnName("spot_number")
                .HasMaxLength(20).IsRequired();
            entity.Property(s => s.Floor).HasColumnName("floor").HasDefaultValue(0);
            entity.Property(s => s.SpotType).HasColumnName("spot_type")
                .HasMaxLength(20).IsRequired();
            entity.Property(s => s.VehicleType).HasColumnName("vehicle_type")
                .HasMaxLength(10).IsRequired();
            entity.Property(s => s.Status).HasColumnName("status")
                .HasMaxLength(20).HasDefaultValue("AVAILABLE");
            entity.Property(s => s.IsHandicapped).HasColumnName("is_handicapped")
                .HasDefaultValue(false);
            entity.Property(s => s.IsEVCharging).HasColumnName("is_ev_charging")
                .HasDefaultValue(false);
            entity.Property(s => s.PricePerHour).HasColumnName("price_per_hour").IsRequired();
            entity.Property(s => s.CreatedAt).HasColumnName("created_at");
            entity.Property(s => s.UpdatedAt).HasColumnName("updated_at");

            // Unique spot number per lot
            entity.HasIndex(s => new { s.LotId, s.SpotNumber }).IsUnique();

            // Index for fast availability queries
            entity.HasIndex(s => new { s.LotId, s.Status });
            entity.HasIndex(s => new { s.LotId, s.SpotType });
            entity.HasIndex(s => new { s.LotId, s.VehicleType });
        });
    }
}
