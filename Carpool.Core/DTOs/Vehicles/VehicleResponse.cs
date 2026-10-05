namespace Carpool.Core.Dtos.Vehicles;

/// <summary>Public projection of a <see cref="Entities.Vehicle"/> (spec §33).</summary>
public class VehicleResponse
{
    public int Id { get; set; }

    public int OwnerId { get; set; }

    public string Manufacturer { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string LicensePlate { get; set; } = string.Empty;

    public int PassengerCapacity { get; set; }
}
