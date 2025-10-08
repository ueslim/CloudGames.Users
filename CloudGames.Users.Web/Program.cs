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
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args).AddObservability();
var configuration = builder.Configuration;

builder.Services.AddApi().AddSwaggerDocs();
builder.Services.AddJwtAuth(configuration);
builder.Services.AddCorsPolicy(builder.Environment); // Added: CORS for local/dev only

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

// Apply EF Core migrations for all contexts before hosted services start
using (var scope = app.Services.CreateScope())
{
    var usersDb = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    usersDb.Database.Migrate();

    var eventDb = scope.ServiceProvider.GetRequiredService<EventStoreSqlContext>();
    eventDb.Database.Migrate();

    var outboxDb = scope.ServiceProvider.GetRequiredService<OutboxContext>();
    outboxDb.Database.Migrate();
}

app.UseSwaggerDocs(app.Environment);
app.UseGlobalExceptionHandling(); // Added: Global exception handling middleware
app.UseCorrelation();
app.UseCorsPolicy(app.Environment); // Added: CORS policy for local/dev only
app.UseHttpsSupport(app.Environment); // Added: HTTPS support (optional)
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
        var secret = _configuration["JwtSettings:Secret"] ?? string.Empty;
        var issuer = _configuration["JwtSettings:Issuer"] ?? "CloudGames";
        var audience = _configuration["JwtSettings:Audience"] ?? "CloudGamesUsers";

        if (string.IsNullOrWhiteSpace(secret))
        {
            return string.Empty;
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

