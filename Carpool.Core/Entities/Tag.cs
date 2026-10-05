namespace Carpool.Core.Entities;

/// <summary>
/// A ride attribute such as "Quiet", "Music", "PetsAllowed", "SmokingForbidden".
/// Beyond the base spec (§30), this project extends tags with a global/private
/// split: a <see cref="OwnerId"/> of <c>null</c> marks a seeded, global tag
/// available to every user; a set <see cref="OwnerId"/> marks a private tag a
/// user created for themselves. <see cref="Name"/> is unique within its scope
/// — once among global tags, and independently once per owner's private set.
/// </summary>
public class Tag
{
    public int Id { get; set; }

    /// <summary>FK to the owning <see cref="User"/>, or <c>null</c> for a global tag.</summary>
    public int? OwnerId { get; set; }

    public string Name { get; set; } = string.Empty;

    // Navigation properties.
    public User? Owner { get; set; }

    /// <summary>Join rows for the Ride ↔ Tag many-to-many relationship (spec §31).</summary>
    public ICollection<RideTag> RideTags { get; set; } = new List<RideTag>();
}
