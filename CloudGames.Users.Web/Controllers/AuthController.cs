using CloudGames.Users.Application.DTOs;
using CloudGames.Users.Application.Users.Commands;
using CloudGames.Users.Application.Users.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CloudGames.Users.Web.Controllers;

/// <summary>
/// Authentication controller for user registration, login, and profile updates
/// </summary>
[ApiController]
[Route("api/users")]
public class AuthController : ControllerBase
{
    private readonly UserCommandHandler _commands;
    public AuthController(UserCommandHandler commands) { _commands = commands; }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="dto">User registration data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created user information</returns>
    /// <response code="201">User successfully created</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="409">Email already exists</response>
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] CreateUserDto dto, CancellationToken ct)
    {
        var user = await _commands.Handle(new RegisterUserCommand(dto), ct);
        // Fixed: Reference GetById method for proper CreatedAtAction with Location header
        return CreatedAtAction(
            actionName: "GetById",
            controllerName: "Users",
            routeValues: new { id = user.Id },
            value: user
        );
    }

    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    /// <param name="dto">Login credentials</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>JWT token and user information</returns>
    /// <response code="200">Authentication successful</response>
    /// <response code="401">Invalid credentials</response>
    [AllowAnonymous]
    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] LoginDto dto, CancellationToken ct)
    {
        var response = await _commands.Handle(dto, ct);
        return Ok(response);
    }

    /// <summary>
    /// Update user profile information
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="dto">Updated user data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated user information</returns>
    /// <response code="200">User successfully updated</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - You can only update your own profile</response>
    /// <response code="404">User not found</response>
    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateUserDto dto, CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
        {
            return Unauthorized();
        }

        var roleClaim = User.FindFirstValue(ClaimTypes.Role) ?? "User";
        
        var user = await _commands.Handle(new UpdateUserCommand(id, dto, currentUserId, roleClaim), ct);
        return Ok(user);
    }
}

