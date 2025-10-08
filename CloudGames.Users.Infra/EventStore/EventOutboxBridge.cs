using System.Text.Json;
using CloudGames.Users.Domain.EventSourcing;
using CloudGames.Users.Domain.Events;
using CloudGames.Users.Infra.Outbox;
using CloudGames.Users.Infra.Persistence;

namespace CloudGames.Users.Infra.EventStore;

public class EventOutboxBridge : IEventStore
{
    private readonly EventStoreSqlContext _eventCtx;
    private readonly OutboxContext _outboxCtx;

    public EventOutboxBridge(EventStoreSqlContext eventCtx, OutboxContext outboxCtx)
    {
        _eventCtx = eventCtx;
        _outboxCtx = outboxCtx;
    }

    public async Task AppendAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var stored = new StoredEvent
        {
            Type = domainEvent.EventType,
            Payload = JsonSerializer.Serialize(domainEvent),
            OccurredAt = domainEvent.OccurredAt
        };
        _eventCtx.StoredEvents.Add(stored);

        var outbox = new OutboxMessage
        {
            Type = domainEvent.EventType,
            Payload = JsonSerializer.Serialize(domainEvent),
            OccurredAt = domainEvent.OccurredAt
        };
        _outboxCtx.OutboxMessages.Add(outbox);

        // Persist both; caller should wrap within its transaction when possible
        await _eventCtx.SaveChangesAsync(cancellationToken);
        await _outboxCtx.SaveChangesAsync(cancellationToken);
    }
}

