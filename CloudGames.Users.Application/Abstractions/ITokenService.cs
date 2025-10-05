using CloudGames.Users.Domain.Entities;

namespace CloudGames.Users.Application.Abstractions;

public interface ITokenService
{
    string GenerateToken(User user);
}

