using CloudGames.Users.Application.Abstractions;
using CloudGames.Users.Application.DTOs;
using CloudGames.Users.Application.Users.Commands;
using CloudGames.Users.Application.Users.Queries;
using CloudGames.Users.Domain.Abstractions;
using CloudGames.Users.Domain.Entities;
using CloudGames.Users.Domain.EventSourcing;
using CloudGames.Users.Domain.Events;
using CloudGames.Users.Domain.Repositories;

namespace CloudGames.Users.Application.Users.Handlers;

public class UserCommandHandler
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IEventStore _eventStore;
    private readonly IUnitOfWork _uow;

    public UserCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEventStore eventStore,
        IUnitOfWork uow)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _eventStore = eventStore;
        _uow = uow;
    }

    public async Task<UserDto> Handle(RegisterUserCommand command, CancellationToken ct)
    {
        var existing = await _users.GetByEmailAsync(command.Dto.Email, ct);
        if (existing != null) throw new InvalidOperationException("Email already in use");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = command.Dto.Name,
            Email = command.Dto.Email,
            PasswordHash = _passwordHasher.Hash(command.Dto.Password),
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        await _eventStore.AppendAsync(new UserRegistered(user.Id, user.Name, user.Email), ct);

        return new UserDto(user.Id, user.Name, user.Email, user.Role.ToString());
    }

    public async Task<UserDto> Handle(UpdateUserCommand command, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(command.Id, ct) ?? throw new KeyNotFoundException("User not found");
        user.Name = command.Dto.Name;
        user.Email = command.Dto.Email;
        user.UpdatedAt = DateTime.UtcNow;
        _users.Update(user);
        await _uow.SaveChangesAsync(ct);

        await _eventStore.AppendAsync(new UserUpdated(user.Id, user.Name, user.Email), ct);

        return new UserDto(user.Id, user.Name, user.Email, user.Role.ToString());
    }

    public async Task<LoginResponseDto> Handle(LoginDto dto, CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(dto.Email, ct);
        if (user == null || !_passwordHasher.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        var token = _tokenService.GenerateToken(user);
        return new LoginResponseDto(token, new UserDto(user.Id, user.Name, user.Email, user.Role.ToString()));
    }
}

public class UserQueryHandler
{
    private readonly IUserRepository _users;
    public UserQueryHandler(IUserRepository users) { _users = users; }

    public async Task<UserDto> Handle(GetUserByIdQuery query, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(query.Id, ct) ?? throw new KeyNotFoundException("User not found");
        return new UserDto(user.Id, user.Name, user.Email, user.Role.ToString());
    }
}

