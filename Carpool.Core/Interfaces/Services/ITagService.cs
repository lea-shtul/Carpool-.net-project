using Carpool.Core.Dtos.Tags;

namespace Carpool.Core.Interfaces.Services;

/// <summary>
/// Tag listing and creation. Beyond the base spec (§30), this project extends tags with a
/// global/private split — see <see cref="Carpool.Core.Entities.Tag"/> — so the service now
/// also lets the authenticated caller create a private tag for themselves.
/// </summary>
public interface ITagService
{
    /// <summary>Global tags plus the caller's own private tags, for <c>GET /api/tags</c>.</summary>
    Task<IEnumerable<TagResponse>> GetVisibleToAsync(int currentUserId, CancellationToken cancellationToken);

    /// <summary>Creates a private tag owned by the caller, for <c>POST /api/tags</c>.</summary>
    Task<TagResponse> CreateAsync(int currentUserId, CreateTagRequest request, CancellationToken cancellationToken);
}
