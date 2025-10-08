using CloudGames.Users.Application.Users.Handlers;
using CloudGames.Users.Application.Users.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CloudGames.Users.Web.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserQueryHandler _queries;
    public UsersController(UserQueryHandler queries) { _queries = queries; }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var user = await _queries.Handle(new GetUserByIdQuery(id), ct);
        return Ok(user);
    }

    // Added: GET /api/users/me endpoint for authenticated user data from JWT token
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

