using Carpool.Core.Enums;

namespace Carpool.Core.Dtos.Bookings;

/// <summary>
/// A booking as seen by the ride's driver — includes the passenger's name, which
/// <see cref="BookingResponse"/> (a passenger's view of their own booking) deliberately
/// omits. Not part of the base spec; backs <c>GET /api/rides/{rideId}/bookings</c>, which
/// only the ride's driver (or an Admin) may call — see SPEC-COMPLIANCE.md.
/// </summary>
public class RideBookingResponse
{
    public int Id { get; set; }

    public int NumberOfSeats { get; set; }

    public BookingStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public RideBookingPassengerInfo Passenger { get; set; } = new();
}

/// <summary>Minimal passenger details surfaced on a driver's view of a ride's bookings.</summary>
public class RideBookingPassengerInfo
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
}
