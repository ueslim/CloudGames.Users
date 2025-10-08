using Microsoft.EntityFrameworkCore;

namespace CloudGames.Users.Infra.EventStore;

public class EventStoreSqlContext : DbContext
{
    public EventStoreSqlContext(DbContextOptions<EventStoreSqlContext> options) : base(options) { }

    public DbSet<StoredEvent> StoredEvents => Set<StoredEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StoredEvent>(b =>
        {
            b.ToTable("StoredEvents");
            b.HasKey(x => x.Id);
            b.Property(x => x.Type).IsRequired().HasMaxLength(128);
            b.Property(x => x.Payload).IsRequired();
            b.Property(x => x.OccurredAt).IsRequired();
            b.Property(x => x.Processed).HasDefaultValue(false);
        });
    }
}

