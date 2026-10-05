using Carpool.Core.Entities;
using Carpool.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Carpool.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IRatingRepository"/> (spec §27–§29, §52). Registered
/// Scoped alongside <see cref="CarpoolDbContext"/>.
///
/// Ratings are write-once: there is no update or delete endpoint, so this repository only
/// reads and adds. The commit happens through <see cref="IUnitOfWork.SaveChangesAsync"/>
/// (spec §68).
/// </summary>
public class RatingRepository : IRatingRepository
{
    private readonly CarpoolDbContext _context;

    public RatingRepository(CarpoolDbContext context)
    {
        _context = context;
    }

    /// <summary>Read-only load — nothing mutates a rating once written.</summary>
    public Task<Rating?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        _context.Ratings
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    /// <summary>
    /// True if this reviewer already rated this ride. Backs the Service-layer check that
    /// sits in front of the <c>(RideId, ReviewerId)</c> unique index (spec §28).
    /// </summary>
    public Task<bool> ExistsForReviewerAsync(int rideId, int reviewerId, CancellationToken cancellationToken) =>
        _context.Ratings.AnyAsync(
            r => r.RideId == rideId && r.ReviewerId == reviewerId,
            cancellationToken);

    /// <summary>Ratings received by a driver, newest first — for <c>GET /api/users/{userId}/ratings</c> (spec §29).</summary>
    public async Task<IEnumerable<Rating>> GetByDriverAsync(int driverId, CancellationToken cancellationToken) =>
        await _context.Ratings
            .AsNoTracking()
            .Where(r => r.DriverId == driverId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Rating rating, CancellationToken cancellationToken) =>
        await _context.Ratings.AddAsync(rating, cancellationToken);
}
