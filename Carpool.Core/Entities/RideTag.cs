namespace Carpool.Core.Entities;

/// <summary>
/// Join entity for the many-to-many relationship between <see cref="Ride"/> and
/// <see cref="Tag"/> (spec §31, §32). Configured with a composite primary key
/// <c>(RideId, TagId)</c> via Fluent API.
/// </summary>
public class RideTag
{
    public int RideId { get; set; }

    public int TagId { get; set; }

    // Navigation properties.
    public Ride Ride { get; set; } = null!;

    public Tag Tag { get; set; } = null!;
}
