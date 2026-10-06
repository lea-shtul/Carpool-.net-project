using Carpool.Core.Dtos.Bookings;
using Carpool.Core.Enums;

namespace Carpool.Core.Interfaces.Services;

/// <summary>
/// Seat booking against the limited-resource ride, and booking cancellation
/// (spec §22–§26). The create path uses optimistic concurrency: on a conflicting
/// concurrent booking it surfaces a concurrency conflict that the API maps to 409.
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// <c>POST /api/rides/{rideId}/bookings</c>. Validates the ride and seat
    /// availability, then decrements <c>AvailableSeats</c> and inserts the booking in
    /// one transaction guarded by the ride's concurrency token (spec §23, §24, §68).
    /// </summary>
    Task<BookingResponse> CreateAsync(int rideId, int currentUserId, CreateBookingRequest request, CancellationToken cancellationToken);

    /// <summary>Backs <c>GET /api/bookings/my</c>.</summary>
    Task<IEnumerable<BookingResponse>> GetMineAsync(int currentUserId, CancellationToken cancellationToken);

    Task<BookingResponse> GetByIdAsync(int bookingId, int currentUserId, UserRole currentUserRole, CancellationToken cancellationToken);

    /// <summary>
    /// <c>GET /api/rides/{rideId}/bookings</c>. Every booking on the ride, passenger names
    /// included. The ride's driver, or an Admin, only — not part of the base spec.
    /// </summary>
    Task<IEnumerable<RideBookingResponse>> GetForRideAsync(
        int rideId, int currentUserId, UserRole currentUserRole, CancellationToken cancellationToken);

    /// <summary>
    /// <c>PATCH /api/bookings/{id}/cancel</c>. Owner only; allowed only while the
    /// booking is Active and the ride is still Scheduled. Transactionally sets the
    /// booking to Cancelled and returns its seats to the ride (spec §25, §68).
    /// </summary>
    Task<BookingResponse> CancelAsync(int bookingId, int currentUserId, CancellationToken cancellationToken);
}
