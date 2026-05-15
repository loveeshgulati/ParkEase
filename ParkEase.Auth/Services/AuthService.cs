using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth;
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
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    private const string RoleManager = "MANAGER";
    private const string RoleAdmin = "ADMIN";
    private const string RoleDriver = "DRIVER";
    private const string StatusPendingApproval = "PENDING_APPROVAL";
    private const string StatusActive = "ACTIVE";
    private const string StatusRejected = "REJECTED";
    private const string StatusSuspended = "SUSPENDED";

    public AuthService(
        IUserRepository userRepository,
        IPublishEndpoint publishEndpoint,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
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
        if (string.Equals(request.Role, RoleAdmin, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cannot register as Admin.");

        var role = request.Role.ToUpper();

        // Drivers get ACTIVE immediately
        // Managers get PENDING_APPROVAL
        var status = role == RoleManager ? StatusPendingApproval : StatusActive;

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone,
            Role = role,
            Status = status,
            IsActive = role != RoleManager,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _userRepository.CreateAsync(user);

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
        if (role == RoleManager)
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

        var message = role == RoleManager
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

        CheckUserStatus(user, isNewRegistration: false);

        var (accessToken, expiry) = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user);

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

        if (!string.IsNullOrWhiteSpace(request.FullName)) user.FullName = request.FullName;
        if (!string.IsNullOrWhiteSpace(request.Phone)) user.Phone = request.Phone;
        if (!string.IsNullOrWhiteSpace(request.ProfilePicUrl)) user.ProfilePicUrl = request.ProfilePicUrl;
        if (!string.IsNullOrWhiteSpace(request.VehiclePlate)) user.VehiclePlate = request.VehiclePlate;

        await _userRepository.UpdateAsync(user);

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
    }

    // ── Deactivate Account ────────────────────────────────────────────────────
    public async Task DeactivateAccountAsync(int userId)
    {
        var user = await _userRepository.FindByUserIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        user.IsActive = false;
        user.Status = StatusSuspended;
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userRepository.UpdateAsync(user);

        // Triggers AccountDeactivationSaga
        await _publishEndpoint.Publish(new UserDeactivatedEvent
        {
            UserId = user.UserId,
            Email = user.Email,
            DeactivatedAt = DateTime.UtcNow
        });

        _logger.LogInformation("User {UserId} deactivated. Saga triggered.", userId);
    }

    // ── Google OAuth ──────────────────────────────────────────────────────────
    public async Task<LoginResponseDto> GoogleAuthAsync(string idToken, string role)
    {
        // 1. Verify the token against Google's public keys
        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _configuration["Google:ClientId"]! }
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
        catch (InvalidJwtException ex)
        {
            throw new UnauthorizedAccessException($"Invalid Google token: {ex.Message}");
        }

        // 2. Find existing user or create a new one
        var user = await _userRepository.FindByEmailAsync(payload.Email);

        if (user == null)
        {
            // Normalise role
            var normalisedRole = (role ?? RoleDriver).ToUpper();
            if (normalisedRole == RoleAdmin)
                throw new InvalidOperationException("Cannot register as Admin.");

            var status = normalisedRole == RoleManager ? StatusPendingApproval : StatusActive;

            user = new User
            {
                FullName      = payload.Name ?? payload.Email,
                Email         = payload.Email.ToLower(),
                PasswordHash  = string.Empty,          // no password for OAuth users
                Phone         = string.Empty,          // can be filled in later
                Role          = normalisedRole,
                Status        = status,
                IsActive      = normalisedRole != RoleManager,
                ProfilePicUrl = payload.Picture,
                OAuthProvider   = "GOOGLE",
                OAuthProviderId = payload.Subject,
                CreatedAt     = DateTime.UtcNow
            };

            user = await _userRepository.CreateAsync(user);

            // Fire welcome notification
            await _publishEndpoint.Publish(new UserRegisteredEvent
            {
                UserId       = user.UserId,
                FullName     = user.FullName,
                Email        = user.Email,
                Phone        = user.Phone,
                Role         = user.Role,
                RegisteredAt = user.CreatedAt
            });

            if (normalisedRole == RoleManager)
            {
                await _publishEndpoint.Publish(new ManagerSignupRequestedEvent
                {
                    ManagerId   = user.UserId,
                    FullName    = user.FullName,
                    Email       = user.Email,
                    Phone       = user.Phone,
                    RequestedAt = user.CreatedAt
                });
            }

            _logger.LogInformation("Google OAuth: New user created {Email} Role={Role}",
                user.Email, user.Role);
        }
        else
        {
            // Update OAuth fields if this is the first time the user signs in via Google
            if (string.IsNullOrEmpty(user.OAuthProvider))
            {
                user.OAuthProvider   = "GOOGLE";
                user.OAuthProviderId = payload.Subject;
                if (string.IsNullOrEmpty(user.ProfilePicUrl))
                    user.ProfilePicUrl = payload.Picture;
            }
            _logger.LogInformation("Google OAuth: Existing user signed in {Email}", user.Email);
        }

        // 3. Status checks (applies to both new and existing users)
        bool isNewUser = user.CreatedAt >= DateTime.UtcNow.AddSeconds(-5);
        CheckUserStatus(user, isNewUser);

        // 4. Issue ParkEase tokens
        var (accessToken, expiry) = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken       = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user);

        return new LoginResponseDto
        {
            UserId       = user.UserId,
            FullName     = user.FullName,
            Email        = user.Email,
            Role         = user.Role,
            AccessToken  = accessToken,
            RefreshToken = refreshToken,
            TokenExpiry  = expiry
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static void CheckUserStatus(User user, bool isNewRegistration)
    {
        switch (user.Status)
        {
            case StatusPendingApproval:
                if (isNewRegistration)
                    throw new UnauthorizedAccessException("Registration successful. Awaiting admin approval.");
                throw new UnauthorizedAccessException("Your manager account is awaiting admin approval.");
            case StatusRejected:
                throw new UnauthorizedAccessException($"Your application was rejected. Reason: {user.RejectionReason}");
            case StatusSuspended:
                throw new UnauthorizedAccessException("Your account has been suspended. Please contact support.");
        }

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated.");
    }

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
