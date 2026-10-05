using Carpool.Core.Enums;

namespace Carpool.Core.Entities;

/// <summary>
/// A ride offered by a driver (spec §10, §11). The ride is the limited resource
/// at the centre of the concurrency requirement: <see cref="AvailableSeats"/> must
/// never go negative and must never exceed <see cref="TotalSeats"/>.
///
/// There is intentionally no update endpoint for a ride (spec §13); state changes
/// happen only through dedicated cancel/complete actions.
/// </summary>
public class Ride
{
    public int Id { get; set; }

    /// <summary>FK to the driver (<see cref="User"/>).</summary>
    public int DriverId { get; set; }

    /// <summary>FK to the <see cref="Vehicle"/> used for this ride. Non-nullable and immutable.</summary>
    public int VehicleId { get; set; }

    public string Origin { get; set; } = string.Empty;

    public string Destination { get; set; } = string.Empty;

    /// <summary>Planned departure time, stored in UTC (spec §50).</summary>
    public DateTime DepartureTime { get; set; }

    public int TotalSeats { get; set; }

    /// <summary>
    /// Remaining bookable seats. Set to <see cref="TotalSeats"/> on creation, decreased
    /// on a successful booking and increased on a cancellation. Invariant: 0 &lt;= AvailableSeats &lt;= TotalSeats.
    /// </summary>
    public int AvailableSeats { get; set; }

    public decimal PricePerSeat { get; set; }

    public int EstimatedDurationMinutes { get; set; }

    public RideStatus Status { get; set; } = RideStatus.Scheduled;

    public DateTime CreatedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Optimistic concurrency token. Mapped in Fluent API to PostgreSQL's <c>xmin</c>
    /// system column (type <c>xid</c>), value-generated on add and update. Two requests
    /// that read the same ride cannot both save a seat change: the second
    /// <c>SaveChangesAsync</c> throws <c>DbUpdateConcurrencyException</c> → 409 (spec §23, §46).
    /// </summary>
    public uint RowVersion { get; set; }

    // Navigation properties.
    public User Driver { get; set; } = null!;

    public Vehicle Vehicle { get; set; } = null!;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    /// <summary>Join rows for the Ride ↔ Tag many-to-many relationship (spec §31).</summary>
    public ICollection<RideTag> RideTags { get; set; } = new List<RideTag>();

    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
