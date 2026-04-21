using ParkEase.Auth.DTOs;

namespace ParkEase.Auth.Interfaces;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task LogoutAsync(int userId);
    Task<TokenResponseDto> RefreshTokenAsync(string refreshToken);
    Task<bool> ValidateTokenAsync(string token);
    Task<UserProfileDto> GetUserByIdAsync(int userId);
    Task<UserProfileDto> GetUserByEmailAsync(string email);
    Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto request);
    Task ChangePasswordAsync(int userId, ChangePasswordDto request);
    Task DeactivateAccountAsync(int userId);
}
