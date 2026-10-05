using System.Security.Claims;
using Carpool.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Carpool.API.Controllers;

/// <summary>
/// Base class for every API controller. Carries <see cref="ApiControllerAttribute"/> (which
/// gives automatic <c>400</c> on model-validation failure and <c>[FromBody]</c> inference,
/// spec §35) and exposes the authenticated caller's identity from the validated JWT.
///
/// The user id and role come only from the token's claims — never from a route, query or
/// body value — so a client cannot act as another user or claim a role it was not issued
/// (spec §40, §43).
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>The authenticated user's id, from the <c>sub</c> claim.</summary>
    protected int CurrentUserId
    {
        get
        {
            var raw = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return int.TryParse(raw, out var id)
                ? id
                : throw new InvalidOperationException("The authenticated token has no valid 'sub' claim.");
        }
    }

    /// <summary>The authenticated user's role, from the <c>role</c> claim.</summary>
    protected UserRole CurrentUserRole
    {
        get
        {
            var raw = User.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(raw, ignoreCase: true, out var role)
                ? role
                : throw new InvalidOperationException("The authenticated token has no valid 'role' claim.");
        }
    }
}
