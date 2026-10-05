using System.ComponentModel.DataAnnotations;

namespace Carpool.Core.Dtos.Auth;

/// <summary>Body of <c>POST /api/auth/login</c> (spec §39). Never logged (spec §60).</summary>
public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
