using System.ComponentModel.DataAnnotations;

namespace Carpool.Core.Dtos.Bookings;

/// <summary>
/// Body of <c>POST /api/rides/{rideId}/bookings</c> (spec §22). The passenger is
/// taken from the JWT and the ride from the route. Service-layer rules include:
/// ride exists and is Scheduled/not started/not cancelled, enough
/// <c>AvailableSeats</c>, the driver cannot book their own ride, and at most one
/// Active booking per (ride, passenger). Seat decrement + booking insert run in one
/// transaction guarded by optimistic concurrency (spec §23, §68).
/// </summary>
public class CreateBookingRequest
{
    [Range(1, 20)]
    public int NumberOfSeats { get; set; }
}
