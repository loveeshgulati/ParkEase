namespace ParkEase.Auth.DTOs;

/// <summary>Request DTO for Google OAuth — carries the ID token issued by Google Sign-In.</summary>
public class GoogleAuthRequestDto
{
    /// <summary>The ID token returned by Google Identity Services on the frontend.</summary>
    public string IdToken { get; set; } = string.Empty;

    /// <summary>
    /// Role to assign if this is a first-time registration via Google.
    /// Accepted values: DRIVER | MANAGER. Defaults to DRIVER when omitted.
    /// </summary>
    public string Role { get; set; } = "DRIVER";
}
