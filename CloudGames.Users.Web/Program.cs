using Azure.Storage.Queues;
using CloudGames.Users.Application.Abstractions;
using CloudGames.Users.Application.Users.Handlers;
using CloudGames.Users.Domain.Abstractions;
using CloudGames.Users.Domain.EventSourcing;
using CloudGames.Users.Domain.Repositories;
using CloudGames.Users.Infra.EventStore;
using CloudGames.Users.Infra.Outbox;
using CloudGames.Users.Infra.Persistence;
using CloudGames.Users.Infra.Repositories;
using CloudGames.Users.Web.Configuration;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args).AddObservability();
var configuration = builder.Configuration;

builder.Services.AddApi().AddSwaggerDocs();
builder.Services.AddJwtAuth(configuration);

// DbContexts
builder.Services.AddDbContext<UsersDbContext>(o => o.UseSqlServer(configuration.GetConnectionString("UsersDb")));
builder.Services.AddDbContext<EventStoreSqlContext>(o => o.UseSqlServer(configuration.GetConnectionString("UsersDb")));
builder.Services.AddDbContext<OutboxContext>(o => o.UseSqlServer(configuration.GetConnectionString("UsersDb")));

// Infra
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UsersDbContext>());
builder.Services.AddScoped<IEventStore, EventOutboxBridge>();
builder.Services.AddSingleton(sp => new QueueClient(configuration.GetConnectionString("Storage") ?? "UseDevelopmentStorage=true", configuration["Queues:Users"] ?? "users-events"));
builder.Services.AddHostedService<OutboxPublisher>();

// Application services
builder.Services.AddScoped<UserCommandHandler>();
builder.Services.AddScoped<UserQueryHandler>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();

app.UseSwaggerDocs(app.Environment);
app.UseCorrelation();
app.UseApi();

app.Run();

// Simple implementations in Web to keep sample self-contained; move to Infra as needed
public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool Verify(string password, string passwordHash) => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    public TokenService(IConfiguration configuration) { _configuration = configuration; }
    public string GenerateToken(CloudGames.Users.Domain.Entities.User user)
    {
        // In JWKS mode this service is used only if acting as issuer elsewhere; placeholder
        return string.Empty;
    }
}

