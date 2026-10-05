using Carpool.Core.Dtos.Vehicles;
using Carpool.Core.Enums;

namespace Carpool.Core.Interfaces.Services;

/// <summary>
/// Vehicle CRUD with server-side ownership checks (spec §9, §43). The acting
/// user's id and role are passed in explicitly (resolved from the JWT in the
/// controller) so the service never trusts client-supplied identity.
/// </summary>
public interface IVehicleService
{
    Task<VehicleResponse> CreateAsync(int currentUserId, CreateVehicleRequest request, CancellationToken cancellationToken);

    Task<VehicleResponse> GetByIdAsync(int vehicleId, int currentUserId, UserRole currentUserRole, CancellationToken cancellationToken);

    Task<IEnumerable<VehicleResponse>> GetMineAsync(int currentUserId, CancellationToken cancellationToken);

    Task<VehicleResponse> UpdateAsync(int vehicleId, int currentUserId, UserRole currentUserRole, UpdateVehicleRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes the vehicle. Fails with a conflict if any ride references it (spec §9).</summary>
    Task DeleteAsync(int vehicleId, int currentUserId, UserRole currentUserRole, CancellationToken cancellationToken);
}
