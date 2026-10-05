using Carpool.Core.Entities;

namespace Carpool.Core.Interfaces.Security;

/// <summary>
/// Issues a signed JWT for an authenticated user (spec §40). The token carries
/// <c>UserId</c>, <c>Email</c> and <c>Role</c> claims. The concrete implementation
/// (signing key, issuer, audience, lifetime) lives outside Core and is configured
/// through <c>appsettings</c>/User Secrets.
/// </summary>
public interface IJwtTokenGenerator
{
    GeneratedToken Generate(User user);
}

/// <summary>A freshly issued access token and its UTC expiry.</summary>
public sealed record GeneratedToken(string AccessToken, DateTime ExpiresAtUtc);
