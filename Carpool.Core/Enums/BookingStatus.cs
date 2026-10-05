namespace Carpool.Core.Enums;

/// <summary>
/// Lifecycle of a booking (spec §21). A booking row is never physically deleted:
/// Active → Cancelled (passenger cancels, or the ride is cancelled) or
/// Active → Completed (the ride completes).
/// </summary>
public enum BookingStatus
{
    Active = 0,
    Cancelled = 1,
    Completed = 2
}
