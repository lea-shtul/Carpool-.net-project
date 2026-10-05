using Carpool.Core.Entities;

namespace Carpool.Core.Interfaces.Repositories;

/// <summary>
/// Persistence contract for <see cref="Tag"/>. Beyond the base spec (§30), this project
/// extends tags with a global/private split — see <see cref="Tag"/> — so unlike most of
/// this interface's original read-only shape, it now also supports creating a private tag.
/// </summary>
public interface ITagRepository
{
    /// <summary>Global tags plus <paramref name="userId"/>'s own private tags, ordered by name.</summary>
    Task<IEnumerable<Tag>> GetVisibleToAsync(int userId, CancellationToken cancellationToken);

    /// <summary>Loads the tags for the given ids — used to attach tags when a ride is created.</summary>
    Task<IEnumerable<Tag>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);

    /// <summary>
    /// True if a tag named <paramref name="name"/> already exists in the given scope —
    /// among global tags when <paramref name="ownerId"/> is <c>null</c>, or within that
    /// owner's own private tags otherwise. Backs the scoped uniqueness rule.
    /// </summary>
    Task<bool> NameExistsAsync(string name, int? ownerId, CancellationToken cancellationToken);

    Task AddAsync(Tag tag, CancellationToken cancellationToken);
}
