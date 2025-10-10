namespace CloudGames.Users.Application.DTOs;

public record CreateUserDto(string Name, string Email, string Password);
public record UpdateUserDto(string Name, string Email, string? Role = null, bool? IsActive = null);
public record LoginDto(string Email, string Password);
public record UserDto(Guid Id, string Name, string Email, string Role, bool IsActive, DateTime CreatedAt, DateTime? UpdatedAt);
public record LoginResponseDto(string Token, UserDto User);

