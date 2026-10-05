using System.ComponentModel.DataAnnotations;

namespace Carpool.Core.Dtos.Vehicles;

/// <summary>
/// Body of <c>POST /api/vehicles</c> (spec §9). The owner is taken from the JWT,
/// never from the request (spec §43, §74). LicensePlate uniqueness is a Service-layer rule.
/// </summary>
public class CreateVehicleRequest
{
    [Required]
    [StringLength(60)]
    public string Manufacturer { get; set; } = string.Empty;

    [Required]
    [StringLength(60)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string LicensePlate { get; set; } = string.Empty;

    [Range(1, 20)]
    public int PassengerCapacity { get; set; }
}
