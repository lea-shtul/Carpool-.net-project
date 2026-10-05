using Carpool.Core.Dtos.Common;

namespace Carpool.Core.Dtos.Rides;

/// <summary>
/// Result of <c>GET /api/rides</c> (spec §20, §33): a single page of <see cref="RideResponse"/>
/// items together with paging metadata.
/// </summary>
public class RideListResponse : PagedResult<RideResponse>
{
}
