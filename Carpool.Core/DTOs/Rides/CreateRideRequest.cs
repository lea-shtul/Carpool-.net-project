using System.ComponentModel.DataAnnotations;

namespace Carpool.Core.Dtos.Rides;

/// <summary>
/// Body of <c>POST /api/rides</c> (spec §12). The driver is taken from the JWT.
/// Service-layer rules (spec §11, §12, §67): the vehicle must exist and be owned by
/// the authenticated user; <c>TotalSeats</c> must be &gt; 0 and not exceed the
/// vehicle's <c>PassengerCapacity</c>; <c>AvailableSeats</c> is initialised to <c>TotalSeats</c>.
/// </summary>
public class CreateRideRequest
{
    [Range(1, int.MaxValue)]
    public int VehicleId { get; set; }

    [Required]
    [StringLength(120)]
    public string Origin { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Destination { get; set; } = string.Empty;

    /// <summary>Planned departure time. Interpreted/stored as UTC (spec §50).</summary>
    [Required]
    public DateTime DepartureTime { get; set; }

    [Range(1, 20)]
    public int TotalSeats { get; set; }

    [Range(0, 100000)]
    public decimal PricePerSeat { get; set; }

    [Range(1, 24 * 60)]
    public int EstimatedDurationMinutes { get; set; }

    /// <summary>Optional predefined tags to attach to the ride (spec §30, §31).</summary>
    public List<int> TagIds { get; set; } = new();
}
