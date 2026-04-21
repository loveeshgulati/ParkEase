namespace ParkEase.Auth.Entities;

public class User
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    // DRIVER | MANAGER | ADMIN
    public string Role { get; set; } = "DRIVER";

    // ACTIVE | PENDING_APPROVAL | REJECTED | SUSPENDED
    public string Status { get; set; } = "ACTIVE";

    public string? RejectionReason { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedByAdminId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ProfilePicUrl { get; set; }
    public string? VehiclePlate { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public string? OAuthProvider { get; set; }
    public string? OAuthProviderId { get; set; }

    public override string ToString() =>
        $"User[{UserId}] {FullName} <{Email}> Role={Role} Status={Status}";
}
