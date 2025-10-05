using CloudGames.Users.Application.DTOs;
using CloudGames.Users.Application.Users.Commands;
using CloudGames.Users.Application.Users.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudGames.Users.Web.Controllers;

[ApiController]
[Route("api/users")]
public class AuthController : ControllerBase
{
    private readonly UserCommandHandler _commands;
    public AuthController(UserCommandHandler commands) { _commands = commands; }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] CreateUserDto dto, CancellationToken ct)
    {
        var user = await _commands.Handle(new RegisterUserCommand(dto), ct);
        return CreatedAtAction(nameof(Register), user);
    }

    [AllowAnonymous]
    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] LoginDto dto, CancellationToken ct)
    {
        var response = await _commands.Handle(dto, ct);
        return Ok(response);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateUserDto dto, CancellationToken ct)
    {
        var user = await _commands.Handle(new UpdateUserCommand(id, dto), ct);
        return Ok(user);
    }
}

