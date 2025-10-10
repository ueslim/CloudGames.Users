using CloudGames.Users.Application.Users.Handlers;
using CloudGames.Users.Application.Users.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CloudGames.Users.Web.Controllers;

/// <summary>
/// Users controller for retrieving user information
/// </summary>
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserQueryHandler _queries;
    public UsersController(UserQueryHandler queries) { _queries = queries; }

    /// <summary>
    /// Get all users (Administrator only)
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of all users</returns>
    /// <response code="200">Users retrieved successfully</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Administrator role required</response>
    [Authorize(Roles = "Administrator")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var users = await _queries.Handle(new GetAllUsersQuery(), ct);
        return Ok(users);
    }

    /// <summary>
    /// Get user information by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>User information</returns>
    /// <response code="200">User found</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="404">User not found</response>
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var user = await _queries.Handle(new GetUserByIdQuery(id), ct);
        return Ok(user);
    }

    /// <summary>
    /// Get current authenticated user information from JWT token
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Current user information</returns>
    /// <response code="200">User information retrieved</response>
    /// <response code="401">Unauthorized - JWT token required or invalid</response>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _queries.Handle(new GetUserByIdQuery(userId), ct);
        return Ok(user);
    }
}

