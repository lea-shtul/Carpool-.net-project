using Carpool.Core.Dtos.Auth;
using Carpool.Core.Dtos.Users;

namespace Carpool.Core.Interfaces.Services;

/// <summary>Registration and login (spec §39, §40).</summary>
public interface IAuthService
{
    /// <summary>Creates a user with a hashed password. Fails if the email is already taken.</summary>
    Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    /// <summary>Validates credentials and returns a signed JWT plus the user profile.</summary>
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
