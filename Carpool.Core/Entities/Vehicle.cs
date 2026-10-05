namespace Carpool.Core.Entities;

/// <summary>
/// A vehicle owned by a user (spec §8). LicensePlate is unique.
/// PassengerCapacity must be positive. A vehicle referenced by ANY ride
/// (regardless of ride status) cannot be deleted (spec §9, §73) — the
/// Ride → Vehicle relationship is deliberately non-nullable to preserve history.
/// </summary>
public class Vehicle
{
    public int Id { get; set; }

    /// <summary>FK to the owning <see cref="User"/>.</summary>
    public int OwnerId { get; set; }

    public string Manufacturer { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string LicensePlate { get; set; } = string.Empty;

    public int PassengerCapacity { get; set; }

    // Navigation properties.
    public User Owner { get; set; } = null!;

    /// <summary>Every ride that has used this vehicle over time (one-to-many).</summary>
    public ICollection<Ride> Rides { get; set; } = new List<Ride>();
}
