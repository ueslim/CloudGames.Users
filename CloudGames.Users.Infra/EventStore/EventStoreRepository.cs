using System.Text.Json;
using CloudGames.Users.Domain.EventSourcing;
using CloudGames.Users.Domain.Events;

namespace CloudGames.Users.Infra.EventStore;

public class EventStoreRepository : IEventStore
{
    private readonly EventStoreSqlContext _ctx;
    public EventStoreRepository(EventStoreSqlContext ctx) { _ctx = ctx; }

    public async Task AppendAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var stored = new StoredEvent
        {
            Type = domainEvent.EventType,
            Payload = JsonSerializer.Serialize(domainEvent),
            OccurredAt = domainEvent.OccurredAt,
            Processed = false
        };
        _ctx.StoredEvents.Add(stored);
        await _ctx.SaveChangesAsync(cancellationToken);
    }
}

