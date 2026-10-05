using System.ComponentModel.DataAnnotations;

namespace Carpool.Core.Dtos.Auth;

/// <summary>
/// Body of <c>POST /api/auth/register</c> (spec §39). Model-level checks run via
/// Data Annotations + ModelState (spec §35); uniqueness of the email is a business
/// rule enforced in the Service layer.
/// </summary>
public class RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;
}
