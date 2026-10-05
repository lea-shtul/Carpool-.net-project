using Carpool.Core.Dtos.Users;

namespace Carpool.Core.Interfaces.Services;

/// <summary>User self-service and Admin user management (spec §42, §41).</summary>
public interface IUserService
{
    /// <summary>Backs <c>GET /api/users/me</c> and Admin lookups by id.</summary>
    Task<UserResponse> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>Backs <c>GET /api/users</c> — Admin only.</summary>
    Task<IEnumerable<UserResponse>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>Backs <c>PATCH /api/users/{id}/status</c> — Admin only (activate/deactivate).</summary>
    Task<UserResponse> SetStatusAsync(int userId, bool isActive, CancellationToken cancellationToken);
}
