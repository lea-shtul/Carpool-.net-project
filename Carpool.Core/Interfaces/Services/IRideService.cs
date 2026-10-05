using Carpool.Core.Dtos.Rides;
using Carpool.Core.Enums;

namespace Carpool.Core.Interfaces.Services;

/// <summary>
/// Ride creation, search and lifecycle actions (spec §12, §13, §17, §18, §20).
/// There is no update method by design — a ride is immutable after creation.
/// </summary>
public interface IRideService
{
    Task<RideResponse> CreateAsync(int currentUserId, CreateRideRequest request, CancellationToken cancellationToken);

    /// <summary>Backs <c>GET /api/rides/{id}</c>. A full ride can still be retrieved directly (spec §19).</summary>
    Task<RideResponse> GetByIdAsync(int rideId, CancellationToken cancellationToken);

    /// <summary>Backs <c>GET /api/rides</c> — filtered, sorted, database-paged (spec §20).</summary>
    Task<RideListResponse> SearchAsync(RideQueryParameters query, CancellationToken cancellationToken);

    /// <summary>
    /// <c>PATCH /api/rides/{id}/cancel</c>. Driver (or Admin) only; allowed only while
    /// Scheduled. Transactionally sets the ride to Cancelled and cancels all Active
    /// bookings (spec §18, §68).
    /// </summary>
    Task<RideResponse> CancelAsync(int rideId, int currentUserId, UserRole currentUserRole, CancellationToken cancellationToken);

    /// <summary>
    /// <c>PATCH /api/rides/{id}/complete</c>. Driver (or Admin) only. Transactionally
    /// sets the ride to Completed, stamps <c>CompletedAt</c> and completes all Active
    /// bookings (spec §17, §14, §68).
    /// </summary>
    Task<RideResponse> CompleteAsync(int rideId, int currentUserId, UserRole currentUserRole, CancellationToken cancellationToken);
}
