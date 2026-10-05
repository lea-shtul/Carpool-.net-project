using System.ComponentModel.DataAnnotations;

namespace Carpool.Core.Dtos.Vehicles;

/// <summary>
/// Body of <c>PUT /api/vehicles/{id}</c> (spec §9). Ownership is verified in the
/// Service layer: a normal user may only update their own vehicle (spec §9, §43).
/// </summary>
public class UpdateVehicleRequest
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
