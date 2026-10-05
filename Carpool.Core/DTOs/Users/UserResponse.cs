using Carpool.Core.Enums;

namespace Carpool.Core.Dtos.Users;

/// <summary>
/// Public projection of a <see cref="Entities.User"/> (spec §33). Deliberately
/// omits <c>PasswordHash</c> and every other sensitive field (spec §66, §74).
/// </summary>
public class UserResponse
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
