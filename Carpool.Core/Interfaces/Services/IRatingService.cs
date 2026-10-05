using Carpool.Core.Dtos.Ratings;

namespace Carpool.Core.Interfaces.Services;

/// <summary>Driver ratings by passengers (spec §27–§29).</summary>
public interface IRatingService
{
    /// <summary>
    /// <c>POST /api/rides/{rideId}/ratings</c>. Requires a Completed ride, the reviewer's
    /// own Completed booking on it, reviewer != driver, and no existing rating from
    /// this reviewer for this ride (spec §27, §28).
    /// </summary>
    Task<RatingResponse> CreateAsync(int rideId, int currentUserId, CreateRatingRequest request, CancellationToken cancellationToken);

    /// <summary>Backs <c>GET /api/users/{userId}/ratings</c> — ratings received by that user as a driver.</summary>
    Task<IEnumerable<RatingResponse>> GetForUserAsync(int userId, CancellationToken cancellationToken);
}
