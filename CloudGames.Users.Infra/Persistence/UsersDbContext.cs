using CloudGames.Users.Domain.Abstractions;
using CloudGames.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloudGames.Users.Infra.Persistence;

public class UsersDbContext : DbContext, IUnitOfWork
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("Users");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Email).IsRequired().HasMaxLength(100);
            b.HasIndex(x => x.Email).IsUnique();
            b.Property(x => x.PasswordHash).IsRequired();
            b.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
            b.Property(x => x.IsActive).HasDefaultValue(true);
        });
    }
}

