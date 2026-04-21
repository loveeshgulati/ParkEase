using Microsoft.EntityFrameworkCore;
using ParkEase.Vehicle.Entities;

namespace ParkEase.Vehicle.Data;

public class VehicleDbContext : DbContext
{
    public VehicleDbContext(DbContextOptions<VehicleDbContext> options) : base(options) { }

    public DbSet<Entities.Vehicle> Vehicles => Set<Entities.Vehicle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Entities.Vehicle>(entity =>
        {
            entity.ToTable("vehicles");
            entity.HasKey(v => v.VehicleId);
            entity.Property(v => v.VehicleId).HasColumnName("vehicle_id").UseIdentityColumn();
            entity.Property(v => v.OwnerId).HasColumnName("owner_id").IsRequired();
            entity.Property(v => v.LicensePlate).HasColumnName("license_plate").HasMaxLength(20).IsRequired();
            entity.Property(v => v.Make).HasColumnName("make").HasMaxLength(50).IsRequired();
            entity.Property(v => v.Model).HasColumnName("model").HasMaxLength(50).IsRequired();
            entity.Property(v => v.Color).HasColumnName("color").HasMaxLength(30).IsRequired();
            entity.Property(v => v.VehicleType).HasColumnName("vehicle_type").HasMaxLength(10).IsRequired();
            entity.Property(v => v.IsEV).HasColumnName("is_ev").HasDefaultValue(false);
            entity.Property(v => v.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(v => v.RegisteredAt).HasColumnName("registered_at");

            // Unique license plate per owner
            entity.HasIndex(v => new { v.OwnerId, v.LicensePlate }).IsUnique();
        });
    }
}
