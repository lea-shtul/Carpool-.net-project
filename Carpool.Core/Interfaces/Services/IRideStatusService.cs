namespace Carpool.Core.Interfaces.Services;

/// <summary>
/// Time-driven ride status reconciliation (spec §15), invoked by the hosted
/// <c>RideStatusBackgroundService</c> on a timer. These are system operations with no
/// caller identity — distinct from <see cref="IRideService"/>, whose <c>CompleteAsync</c>
/// is an ownership-checked user action.
/// </summary>
public interface IRideStatusService
{
    /// <summary>
    /// Moves every <c>Scheduled</c> ride whose <c>DepartureTime</c> has passed to
    /// <c>InProgress</c> and sets <c>StartedAt</c>. Returns the number transitioned.
    /// </summary>
    Task<int> StartDueRidesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Moves every <c>InProgress</c> ride whose <c>DepartureTime + EstimatedDurationMinutes</c>
    /// has passed to <c>Completed</c>, sets <c>CompletedAt</c>, and transitions all of that
    /// ride's <c>Active</c> bookings to <c>Completed</c> in the same transaction (spec §14).
    /// Returns the number of rides transitioned.
    /// </summary>
    Task<int> CompleteDueRidesAsync(CancellationToken cancellationToken);
}
