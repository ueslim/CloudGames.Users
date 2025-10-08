using System.Text.Json;
using Azure.Storage.Queues;
using CloudGames.Users.Infra.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CloudGames.Users.Infra.Outbox;

public class OutboxPublisher : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<OutboxPublisher> _logger;
    private readonly QueueClient _queue;

    public OutboxPublisher(IServiceProvider provider, ILogger<OutboxPublisher> logger, QueueClient queue)
    {
        _provider = provider;
        _logger = logger;
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _queue.CreateIfNotExistsAsync(cancellationToken: stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _provider.CreateScope();
                var outbox = scope.ServiceProvider.GetRequiredService<OutboxContext>();
                var pending = await outbox.OutboxMessages
                    .Where(o => o.ProcessedAt == null)
                    .OrderBy(o => o.OccurredAt)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                foreach (var msg in pending)
                {
                    await _queue.SendMessageAsync(JsonSerializer.Serialize(new { msg.Type, msg.Payload }), cancellationToken: stoppingToken);
                    msg.ProcessedAt = DateTime.UtcNow;
                }
                if (pending.Count > 0)
                {
                    await outbox.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox publishing failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}

