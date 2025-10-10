using CloudGames.Users.Application.DTOs;

namespace CloudGames.Users.Application.Users.Commands;

public record UpdateUserCommand(Guid Id, UpdateUserDto Dto, Guid CurrentUserId, string CurrentUserRole);

