using Carpool.Core.Enums;

namespace Carpool.Core.Entities;

/// <summary>
/// A passenger's reservation of one or more seats on a ride (spec §21, §22).
/// A user may have at most one <see cref="BookingStatus.Active"/> booking per ride
/// (spec §26). Booking rows are never deleted — cancelling only changes the status
/// so historical data is preserved (spec §21, §73).
/// </summary>
public class Booking
{
    public int Id { get; set; }

    /// <summary>FK to the <see cref="Ride"/> being booked.</summary>
    public int RideId { get; set; }

    /// <summary>FK to the passenger (<see cref="User"/>).</summary>
    public int PassengerId { get; set; }

    public int NumberOfSeats { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Active;

    public DateTime CreatedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    // Navigation properties.
    public Ride Ride { get; set; } = null!;

    public User Passenger { get; set; } = null!;
}
