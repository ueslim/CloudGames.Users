using CloudGames.Users.Domain.Entities;

namespace CloudGames.Users.Domain.Events;

public record UserRegistered(Guid UserId, string Name, string Email) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string EventType => nameof(UserRegistered);
}

