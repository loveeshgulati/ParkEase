using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MassTransit;
using Microsoft.IdentityModel.Tokens;
using ParkEase.Auth.DTOs;
using ParkEase.Auth.Entities;
using ParkEase.Auth.Events;
using ParkEase.Auth.Interfaces;

namespace ParkEase.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        IPublishEndpoint publishEndpoint,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
        _logger = logger;
    }

    // ── Register ──────────────────────────────────────────────────────────────
    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email))
            throw new InvalidOperationException($"Email '{request.Email}' is already registered.");

        // Prevent registering as ADMIN via API
        if (request.Role.ToUpper() == "ADMIN")
            throw new InvalidOperationException("Cannot register as Admin.");

        var role = request.Role.ToUpper();

        // Drivers get ACTIVE immediately
        // Managers get PENDING_APPROVAL
        var status = role == "MANAGER" ? "PENDING_APPROVAL" : "ACTIVE";

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone,
            Role = role,
            Status = status,
            IsActive = role != "MANAGER",
            CreatedAt = DateTime.UtcNow
        };

        var created = await _userRepository.CreateAsync(user);

        await _auditLogRepository.CreateAsync(new AuditLog
        {
            ActorUserId = created.UserId,
            Action = "REGISTER",
            TargetUserId = created.UserId.ToString(),
            After = System.Text.Json.JsonSerializer.Serialize(
                new { created.Email, created.Role, created.Status }),
            Timestamp = DateTime.UtcNow,
            Success = true
        });

        // Fire-and-forget welcome notification
        await _publishEndpoint.Publish(new UserRegisteredEvent
        {
            UserId = created.UserId,
            FullName = created.FullName,
            Email = created.Email,
            Phone = created.Phone,
            Role = created.Role,
            RegisteredAt = created.CreatedAt
        });

        // If manager — notify admin of pending request
        if (role == "MANAGER")
        {
            await _publishEndpoint.Publish(new ManagerSignupRequestedEvent
            {
                ManagerId = created.UserId,
                FullName = created.FullName,
                Email = created.Email,
                Phone = created.Phone,
                RequestedAt = created.CreatedAt
            });
        }

        var message = role == "MANAGER"
            ? "Registration successful. Awaiting admin approval."
            : "Registration successful.";

        _logger.LogInformation("User registered: {Email} Role={Role} Status={Status}",
            created.Email, created.Role, created.Status);

        return new RegisterResponseDto
        {
            UserId = created.UserId,
            FullName = created.FullName,
            Email = created.Email,
            Role = created.Role,
            Status = created.Status,
            Message = message
        };
    }

    // ── Login ─────────────────────────────────────────────────────────────────
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        // Status checks
        switch (user.Status)
        {
            case "PENDING_APPROVAL":
                throw new UnauthorizedAccessException(
                    "Your manager account is awaiting admin approval.");
            case "REJECTED":
                throw new UnauthorizedAccessException(
                    $"Your application was rejected. Reason: {user.RejectionReason}");
            case "SUSPENDED":
                throw new UnauthorizedAccessException(
                    "Your account has been suspended. Please contact support.");
        }

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated.");

        var (accessToken, expiry) = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user);

        await _auditLogRepository.CreateAsync(new AuditLog
        {
            ActorUserId = user.UserId,
            Action = "LOGIN",
            TargetUserId = user.UserId.ToString(),
            Timestamp = DateTime.UtcNow,
            Success = true
        });

        return new LoginResponseDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenExpiry = expiry
        };
    }

    // ── Logout ────────────────────────────────────────────────────────────────
    public async Task LogoutAsync(int userId)
    {
        var user = await _userRepository.FindByUserIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userRepository.UpdateAsync(user);

        await _auditLogRepository.CreateAsync(new AuditLog
        {
            ActorUserId = userId,
            Action = "LOGOUT",
            TargetUserId = userId.ToString(),
            Timestamp = DateTime.UtcNow,
            Success = true
        });
    }

    // ── Refresh Token ─────────────────────────────────────────────────────────
    public async Task<TokenResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var user = await _userRepository.FindByRefreshTokenAsync(refreshToken)
            ?? throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var (accessToken, expiry) = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user);

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            TokenExpiry = expiry
        };
    }

    // ── Validate Token ────────────────────────────────────────────────────────
    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!);
            var handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);
            return Task.FromResult(true);
        }
        catch { return Task.FromResult(false); }
    }

    // ── Get Profile ───────────────────────────────────────────────────────────
    public async Task<UserProfileDto> GetUserByIdAsync(int userId)
    {
        var user = await _userRepository.FindByUserIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");
        return MapToProfileDto(user);
    }

    public async Task<UserProfileDto> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.FindByEmailAsync(email)
            ?? throw new KeyNotFoundException($"User with email '{email}' not found.");
        return MapToProfileDto(user);
    }

    // ── Update Profile ────────────────────────────────────────────────────────
    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto request)
    {
        var user = await _userRepository.FindByUserIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        var before = System.Text.Json.JsonSerializer.Serialize(
            new { user.FullName, user.Phone, user.VehiclePlate });

        if (!string.IsNullOrWhiteSpace(request.FullName)) user.FullName = request.FullName;
        if (!string.IsNullOrWhiteSpace(request.Phone)) user.Phone = request.Phone;
        if (!string.IsNullOrWhiteSpace(request.ProfilePicUrl)) user.ProfilePicUrl = request.ProfilePicUrl;
        if (!string.IsNullOrWhiteSpace(request.VehiclePlate)) user.VehiclePlate = request.VehiclePlate;

        await _userRepository.UpdateAsync(user);

        await _auditLogRepository.CreateAsync(new AuditLog
        {
            ActorUserId = userId,
            Action = "PROFILE_UPDATE",
            TargetUserId = userId.ToString(),
            Before = before,
            After = System.Text.Json.JsonSerializer.Serialize(
                new { user.FullName, user.Phone, user.VehiclePlate }),
            Timestamp = DateTime.UtcNow,
            Success = true
        });

        await _publishEndpoint.Publish(new UserProfileUpdatedEvent
        {
            UserId = userId,
            FullName = user.FullName,
            Phone = user.Phone,
            VehiclePlate = user.VehiclePlate,
            UpdatedAt = DateTime.UtcNow
        });

        return MapToProfileDto(user);
    }

    // ── Change Password ───────────────────────────────────────────────────────
    public async Task ChangePasswordAsync(int userId, ChangePasswordDto request)
    {
        var user = await _userRepository.FindByUserIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _userRepository.UpdateAsync(user);

        await _auditLogRepository.CreateAsync(new AuditLog
        {
            ActorUserId = userId,
            Action = "PASSWORD_CHANGE",
            TargetUserId = userId.ToString(),
            Timestamp = DateTime.UtcNow,
            Success = true
        });
    }

    // ── Deactivate Account ────────────────────────────────────────────────────
    public async Task DeactivateAccountAsync(int userId)
    {
        var user = await _userRepository.FindByUserIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        user.IsActive = false;
        user.Status = "SUSPENDED";
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userRepository.UpdateAsync(user);

        await _auditLogRepository.CreateAsync(new AuditLog
        {
            ActorUserId = userId,
            Action = "DEACTIVATE",
            TargetUserId = userId.ToString(),
            Before = System.Text.Json.JsonSerializer.Serialize(new { IsActive = true }),
            After = System.Text.Json.JsonSerializer.Serialize(new { IsActive = false }),
            Timestamp = DateTime.UtcNow,
            Success = true
        });

        // Triggers AccountDeactivationSaga
        await _publishEndpoint.Publish(new UserDeactivatedEvent
        {
            UserId = user.UserId,
            Email = user.Email,
            DeactivatedAt = DateTime.UtcNow
        });

        _logger.LogInformation("User {UserId} deactivated. Saga triggered.", userId);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private (string token, DateTime expiry) GenerateJwtToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!);
        var expiry = DateTime.UtcNow.AddHours(24);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("userId", user.UserId.ToString()),
            new Claim("status", user.Status)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256)
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public static UserProfileDto MapToProfileDto(User user) => new()
    {
        UserId = user.UserId,
        FullName = user.FullName,
        Email = user.Email,
        Phone = user.Phone,
        Role = user.Role,
        Status = user.Status,
        VehiclePlate = user.VehiclePlate,
        ProfilePicUrl = user.ProfilePicUrl,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        ApprovedAt = user.ApprovedAt,
        RejectionReason = user.RejectionReason
    };
}
