using System.ComponentModel.DataAnnotations;

namespace Carpool.Core.Dtos.Tags;

/// <summary>
/// Body of <c>POST /api/tags</c>. Not part of the base spec (§30 says tags are predefined) —
/// this project extends tags with private, per-user tags (see
/// <see cref="Entities.Tag"/>). The owner is taken from the JWT, never from the request
/// (spec §43). Name uniqueness within the caller's own private set is a Service-layer rule.
/// </summary>
public class CreateTagRequest
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
}
