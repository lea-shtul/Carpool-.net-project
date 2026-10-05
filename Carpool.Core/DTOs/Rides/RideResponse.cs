using Carpool.Core.Dtos.Tags;
using Carpool.Core.Enums;

namespace Carpool.Core.Dtos.Rides;

/// <summary>
/// Full projection of a ride (spec §66). Includes driver, vehicle and tag summaries
/// built with <c>Include</c>/<c>ThenInclude</c> to avoid N+1 queries (spec §32).
/// Never exposes <c>PasswordHash</c> or other sensitive fields.
/// </summary>
public class RideResponse
{
    public int Id { get; set; }

    public string Origin { get; set; } = string.Empty;

    public string Destination { get; set; } = string.Empty;

    public DateTime DepartureTime { get; set; }

    public int AvailableSeats { get; set; }

    public int TotalSeats { get; set; }

    public decimal PricePerSeat { get; set; }

    public int EstimatedDurationMinutes { get; set; }

    public RideStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public RideDriverInfo Driver { get; set; } = new();

    public RideVehicleInfo Vehicle { get; set; } = new();

    public List<TagResponse> Tags { get; set; } = new();
}

/// <summary>Minimal driver details surfaced on a ride response.</summary>
public class RideDriverInfo
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
}

/// <summary>Minimal vehicle details surfaced on a ride response.</summary>
public class RideVehicleInfo
{
    public int Id { get; set; }

    public string Manufacturer { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int PassengerCapacity { get; set; }
}
