using Carpool.Core.Entities;
using Carpool.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Carpool.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ITagRepository"/> (spec §30, §52). Registered Scoped
/// alongside <see cref="CarpoolDbContext"/>.
///
/// Beyond the base spec's read-only shape, this project extends tags with a global/private
/// split (see <see cref="Tag"/>), so this repository also stages a new private tag
/// (<see cref="AddAsync"/>) — the commit still happens through
/// <see cref="IUnitOfWork.SaveChangesAsync"/> (spec §68). Every read-only query still
/// materialises with <c>ToListAsync</c> so execution finishes inside the repository (spec §20).
/// </summary>
public class TagRepository : ITagRepository
{
    private readonly CarpoolDbContext _context;

    public TagRepository(CarpoolDbContext context)
    {
        _context = context;
    }

    /// <summary>Global tags plus the caller's own private tags, backing <c>GET /api/tags</c>.</summary>
    public async Task<IEnumerable<Tag>> GetVisibleToAsync(int userId, CancellationToken cancellationToken) =>
        await _context.Tags
            .AsNoTracking()
            .Where(t => t.OwnerId == null || t.OwnerId == userId)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Loads the tags for the given ids (SQL <c>IN</c>) so a new ride can be linked to them
    /// (spec §31). Tracked — the ride-creation service attaches these to the new
    /// <see cref="Ride"/>'s tag navigation, and tracked existing entities stop EF from
    /// trying to re-insert them. The service compares the returned count against
    /// <paramref name="ids"/> to reject unknown tag ids, and checks each tag's
    /// <see cref="Tag.OwnerId"/> to reject a tag the caller does not own.
    /// </summary>
    public async Task<IEnumerable<Tag>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken) =>
        await _context.Tags
            .Where(t => ids.Contains(t.Id))
            .ToListAsync(cancellationToken);

    public async Task<bool> NameExistsAsync(string name, int? ownerId, CancellationToken cancellationToken) =>
        await _context.Tags
            .AnyAsync(t => t.OwnerId == ownerId && t.Name == name, cancellationToken);

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken) =>
        await _context.Tags.AddAsync(tag, cancellationToken);
}
