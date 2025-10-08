namespace CloudGames.Users.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
    string EventType { get; }
}

