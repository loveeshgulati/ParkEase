using Microsoft.EntityFrameworkCore;
using ParkEase.Auth.Data;
using Serilog;

namespace ParkEase.Auth.Data;

public class DatabaseSeeder
{
    public static void SeedDatabase(IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            db.Database.Migrate();

            // Seed single admin if not exists
            if (!db.Users.Any(u => u.Role == "ADMIN"))
            {
                db.Users.Add(new ParkEase.Auth.Entities.User
                {
                    FullName = "Platform Admin",
                    Email = "admin@parkease.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Phone = "0000000000",
                    Role = "ADMIN",
                    Status = "ACTIVE",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
                db.SaveChanges();
                Log.Information("Admin seeded: admin@parkease.com / Admin@123");
            }
        }
    }
}
