using Carpool.Core.Dtos.Vehicles;
using Carpool.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Carpool.API.Controllers;

/// <summary>
/// Vehicle CRUD (spec §9). Every action requires authentication; ownership is enforced in
/// <see cref="IVehicleService"/> against <see cref="ApiControllerBase.CurrentUserId"/> /
/// <see cref="ApiControllerBase.CurrentUserRole"/> — the client never supplies an owner id
/// (spec §43).
/// </summary>
[ApiController]
[Authorize]
[Route("api/vehicles")]
public class VehiclesController : ApiControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    /// <summary>POST /api/vehicles — register a vehicle for the authenticated user.</summary>
    [HttpPost]
    [ProducesResponseType<VehicleResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<VehicleResponse>> Create(
        [FromBody] CreateVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
    }

    /// <summary>GET /api/vehicles/my — the authenticated user's vehicles.</summary>
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<VehicleResponse>>> GetMine(CancellationToken cancellationToken)
    {
        var vehicles = await _vehicleService.GetMineAsync(CurrentUserId, cancellationToken);
        return Ok(vehicles);
    }

    /// <summary>GET /api/vehicles/{id} — one vehicle. Owner or Admin only.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<VehicleResponse>> GetById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleService.GetByIdAsync(id, CurrentUserId, CurrentUserRole, cancellationToken);
        return Ok(vehicle);
    }

    /// <summary>PUT /api/vehicles/{id} — update a vehicle. Owner or Admin only.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<VehicleResponse>> Update(
        [FromRoute] int id,
        [FromBody] UpdateVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleService.UpdateAsync(id, CurrentUserId, CurrentUserRole, request, cancellationToken);
        return Ok(vehicle);
    }

    /// <summary>DELETE /api/vehicles/{id} — delete a vehicle. Owner or Admin; returns 409 if any ride references it.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<VehicleResponse>> Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        await _vehicleService.DeleteAsync(id, CurrentUserId, CurrentUserRole, cancellationToken);
        return NoContent();
    }
}
