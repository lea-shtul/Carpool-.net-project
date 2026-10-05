namespace Carpool.Core.Enums;

/// <summary>
/// Application roles. There is intentionally no separate Driver/Passenger role:
/// a single user can act as both. Only these two roles exist (spec §2, §41).
/// </summary>
public enum UserRole
{
    User = 0,
    Admin = 1
}
