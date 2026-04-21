using ParkEase.Auth.Entities;

namespace ParkEase.Auth.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByUserIdAsync(int userId);
    Task<bool> ExistsByEmailAsync(string email);
    Task<List<User>> FindAllByRoleAsync(string role);
    Task<List<User>> FindAllByRoleAndStatusAsync(string role, string status);
    Task<User?> FindByVehiclePlateAsync(string vehiclePlate);
    Task<User?> FindByPhoneAsync(string phone);
    Task<User?> FindByRefreshTokenAsync(string refreshToken);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteByUserIdAsync(int userId);
    Task<List<User>> GetAllAsync();
    Task<bool> AdminExistsAsync();
}
