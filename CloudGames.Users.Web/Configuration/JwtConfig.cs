using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace CloudGames.Users.Web.Configuration;

public static class JwtConfig
{
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var authority = configuration["Jwt:Authority"];
        var audience = configuration["Jwt:Audience"];
        var secret = configuration["JwtSettings:Secret"];
        var issuer = configuration["JwtSettings:Issuer"];
        var manualAudience = configuration["JwtSettings:Audience"];

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;

            if (!string.IsNullOrWhiteSpace(authority))
            {
                // Azure AD or other OIDC provider mode (Production)
                options.RequireHttpsMetadata = true;
                options.Authority = authority;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    // Let OIDC metadata handle issuer validation
                    ValidateIssuerSigningKey = true
                };
            }
            else if (!string.IsNullOrWhiteSpace(secret) && !string.IsNullOrWhiteSpace(issuer))
            {
                // Manual JWT with symmetric key mode (Development)
                options.RequireHttpsMetadata = false; // Allow HTTP for local development
                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret));
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = manualAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            }
            else
            {
                // Fallback: disable validation for development (INSECURE - only for local dev)
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = false,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false
                };
            }
        });

        return services;
    }
}

