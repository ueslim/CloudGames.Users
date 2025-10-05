using CloudGames.Users.Application.Users.Handlers;
using CloudGames.Users.Application.Users.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
}

