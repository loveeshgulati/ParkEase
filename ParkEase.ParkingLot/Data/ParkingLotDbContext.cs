using Microsoft.EntityFrameworkCore;

namespace ParkEase.ParkingLot.Data;

public class ParkingLotDbContext : DbContext
{
    public ParkingLotDbContext(DbContextOptions<ParkingLotDbContext> options)
        : base(options) { }

    public DbSet<Entities.ParkingLot> ParkingLots => Set<Entities.ParkingLot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Entities.ParkingLot>(entity =>
        {
            entity.ToTable("parking_lots");
            entity.HasKey(l => l.LotId);
            entity.Property(l => l.LotId).HasColumnName("lot_id").UseIdentityColumn();
            entity.Property(l => l.ManagerId).HasColumnName("manager_id").IsRequired();
            entity.Property(l => l.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(l => l.Address).HasColumnName("address").HasMaxLength(255).IsRequired();
            entity.Property(l => l.City).HasColumnName("city").HasMaxLength(100).IsRequired();
            entity.Property(l => l.Latitude).HasColumnName("latitude").IsRequired();
            entity.Property(l => l.Longitude).HasColumnName("longitude").IsRequired();
            entity.Property(l => l.TotalSpots).HasColumnName("total_spots").HasDefaultValue(0);
            entity.Property(l => l.AvailableSpots).HasColumnName("available_spots").HasDefaultValue(0);
            entity.Property(l => l.IsOpen).HasColumnName("is_open").HasDefaultValue(false);
            entity.Property(l => l.OpenTime).HasColumnName("open_time");
            entity.Property(l => l.CloseTime).HasColumnName("close_time");
            entity.Property(l => l.ApprovalStatus).HasColumnName("approval_status")
                .HasMaxLength(30).HasDefaultValue("PENDING_APPROVAL");
            entity.Property(l => l.RejectionReason).HasColumnName("rejection_reason");
            entity.Property(l => l.ApprovedAt).HasColumnName("approved_at");
            entity.Property(l => l.ApprovedByAdminId).HasColumnName("approved_by_admin_id");
            entity.Property(l => l.CreatedAt).HasColumnName("created_at");
            entity.Property(l => l.UpdatedAt).HasColumnName("updated_at");

            // Index on city for fast search
            entity.HasIndex(l => l.City);
            // Index on manager for fast lookup
            entity.HasIndex(l => l.ManagerId);
        });
    }
}
