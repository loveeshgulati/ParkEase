using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ParkEase.Auth.Entities;

namespace ParkEase.Auth.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseNpgsql(connectionString);
        }
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.UserId);
            entity.Property(u => u.UserId).HasColumnName("user_id").UseIdentityColumn();
            entity.Property(u => u.FullName).HasColumnName("full_name").HasMaxLength(100).IsRequired();
            entity.Property(u => u.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
            entity.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(u => u.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(u => u.Role).HasColumnName("role").HasMaxLength(20).HasDefaultValue("DRIVER");
            entity.Property(u => u.Status).HasColumnName("status").HasMaxLength(30).HasDefaultValue("ACTIVE");
            entity.Property(u => u.RejectionReason).HasColumnName("rejection_reason");
            entity.Property(u => u.ApprovedAt).HasColumnName("approved_at");
            entity.Property(u => u.ApprovedByAdminId).HasColumnName("approved_by_admin_id");
            entity.Property(u => u.VehiclePlate).HasColumnName("vehicle_plate").HasMaxLength(20);
            entity.Property(u => u.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(u => u.CreatedAt).HasColumnName("created_at");
            entity.Property(u => u.ProfilePicUrl).HasColumnName("profile_pic_url");
            entity.Property(u => u.OAuthProvider).HasColumnName("oauth_provider").HasMaxLength(20);
            entity.Property(u => u.OAuthProviderId).HasColumnName("oauth_provider_id");
            entity.Property(u => u.RefreshToken).HasColumnName("refresh_token");
            entity.Property(u => u.RefreshTokenExpiry).HasColumnName("refresh_token_expiry");
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(a => a.AuditLogId);
            entity.Property(a => a.AuditLogId).HasColumnName("audit_log_id").UseIdentityColumn();
            entity.Property(a => a.ActorUserId).HasColumnName("actor_user_id");
            entity.Property(a => a.Action).HasColumnName("action").HasMaxLength(50).IsRequired();
            entity.Property(a => a.TargetUserId).HasColumnName("target_user_id");
            entity.Property(a => a.Before).HasColumnName("before").HasColumnType("jsonb");
            entity.Property(a => a.After).HasColumnName("after").HasColumnType("jsonb");
            entity.Property(a => a.IpAddress).HasColumnName("ip_address").HasMaxLength(50);
            entity.Property(a => a.Timestamp).HasColumnName("timestamp");
            entity.Property(a => a.Success).HasColumnName("success").HasDefaultValue(true);
            entity.Property(a => a.FailureReason).HasColumnName("failure_reason");
        });
    }
}
