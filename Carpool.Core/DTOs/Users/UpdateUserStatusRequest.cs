using System.ComponentModel.DataAnnotations;

namespace Carpool.Core.Dtos.Users;

/// <summary>
/// Body of <c>PATCH /api/users/{id}/status</c> — an Admin-only action to
/// activate or deactivate a user account (spec §42, §41).
/// </summary>
public class UpdateUserStatusRequest
{
    [Required]
    public bool IsActive { get; set; }
}
