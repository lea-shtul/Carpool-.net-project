using Carpool.Core.Dtos.Users;

namespace Carpool.Core.Dtos.Auth;

/// <summary>
/// Result of a successful login: the signed JWT, its UTC expiry, and the
/// authenticated user's public profile (spec §39, §40).
/// </summary>
public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public UserResponse User { get; set; } = new();
}
