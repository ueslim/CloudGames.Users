using CloudGames.Users.Infra.Outbox;
using Microsoft.EntityFrameworkCore;

namespace CloudGames.Users.Infra.Persistence;

public class OutboxContext : DbContext
{
    public OutboxContext(DbContextOptions<OutboxContext> options) : base(options) { }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("OutboxMessages");
            b.HasKey(x => x.Id);
            b.Property(x => x.Type).IsRequired().HasMaxLength(128);
            b.Property(x => x.Payload).IsRequired();
            b.Property(x => x.OccurredAt).IsRequired();
        });
    }
}

