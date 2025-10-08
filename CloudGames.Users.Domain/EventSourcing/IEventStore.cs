using CloudGames.Users.Domain.Events;

namespace CloudGames.Users.Domain.EventSourcing;

public interface IEventStore
{
    Task AppendAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);
}

