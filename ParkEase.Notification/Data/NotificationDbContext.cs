using Microsoft.EntityFrameworkCore;
using ParkEase.Notification.Entities;

namespace ParkEase.Notification.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options) { }

    public DbSet<ParkEase.Notification.Entities.Notification> Notifications => Set<ParkEase.Notification.Entities.Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ParkEase.Notification.Entities.Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(n => n.NotificationId);
            entity.Property(n => n.NotificationId)
                .HasColumnName("notification_id").UseIdentityColumn();
            entity.Property(n => n.RecipientId)
                .HasColumnName("recipient_id").IsRequired();
            entity.Property(n => n.Title)
                .HasColumnName("title").HasMaxLength(150).IsRequired();
            entity.Property(n => n.Message)
                .HasColumnName("message").IsRequired();
            entity.Property(n => n.Type)
                .HasColumnName("type").HasMaxLength(30).IsRequired();
            entity.Property<string>("Channel")
                .HasColumnName("channel").HasMaxLength(10).HasDefaultValue("APP");
            entity.Property(n => n.RelatedId).HasColumnName("related_id");
            entity.Property(n => n.RelatedType)
                .HasColumnName("related_type").HasMaxLength(30);
            entity.Property(n => n.IsRead)
                .HasColumnName("is_read").HasDefaultValue(false);
            entity.Property(n => n.SentAt).HasColumnName("sent_at");

            entity.HasIndex(n => n.RecipientId);
            entity.HasIndex(n => new { n.RecipientId, n.IsRead });
        });
    }
}
