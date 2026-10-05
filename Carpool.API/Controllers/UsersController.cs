using Carpool.Core.Dtos.Users;
using Carpool.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Carpool.API.Controllers;

/// <summary>
/// User self-service and Admin user management (spec §42). <c>me</c> is available to any
/// authenticated user; listing users and changing account status are Admin-only (spec §41).
/// </summary>
[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>GET /api/users/me — the authenticated user's own profile.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> GetMe(CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(CurrentUserId, cancellationToken);
        return Ok(user);
    }

    /// <summary>GET /api/users — list all users. Admin only.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    /// <summary>PATCH /api/users/{id}/status — activate or deactivate a user account. Admin only.</summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserResponse>> SetStatus(
        [FromRoute] int id,
        [FromBody] UpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userService.SetStatusAsync(id, request.IsActive, cancellationToken);
        return Ok(user);
    }
}
