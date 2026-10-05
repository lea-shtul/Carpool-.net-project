using Carpool.Core.Entities;

namespace Carpool.Core.Interfaces.Repositories;

/// <summary>Persistence contract for <see cref="Rating"/> (spec §27–§29).</summary>
public interface IRatingRepository
{
    Task<Rating?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>True if this reviewer already rated this ride — one rating per (ride, reviewer) (spec §28).</summary>
    Task<bool> ExistsForReviewerAsync(int rideId, int reviewerId, CancellationToken cancellationToken);

    /// <summary>All ratings received by a driver — for <c>GET /api/users/{userId}/ratings</c> (spec §29).</summary>
    Task<IEnumerable<Rating>> GetByDriverAsync(int driverId, CancellationToken cancellationToken);

    Task AddAsync(Rating rating, CancellationToken cancellationToken);
}
