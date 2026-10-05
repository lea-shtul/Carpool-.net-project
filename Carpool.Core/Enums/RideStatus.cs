namespace Carpool.Core.Enums;

/// <summary>
/// Lifecycle of a ride (spec §10). Transitions:
/// Scheduled → InProgress → Completed (automatic via BackgroundService or manual /complete),
/// Scheduled → Cancelled (manual /cancel, not allowed once InProgress).
/// </summary>
public enum RideStatus
{
    Scheduled = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}
