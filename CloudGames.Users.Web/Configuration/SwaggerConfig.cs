using Microsoft.OpenApi.Models;

namespace CloudGames.Users.Web.Configuration;

public static class SwaggerConfig
{
    public static IServiceCollection AddSwaggerDocs(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            // Enhanced API documentation for APIM compatibility
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "CloudGames Users API", 
                Version = "v1",
                Description = "User management microservice for CloudGames platform",
                Contact = new OpenApiContact
                {
                    Name = "CloudGames Team",
                    Email = "support@cloudgames.com"
                }
            });
            
            // JWT Bearer token security definition for APIM integration
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });
            
            // Global security requirement - all endpoints require JWT except those marked [AllowAnonymous]
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
            
            // Include XML comments if available for better documentation
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });
        return services;
    }

    public static IApplicationBuilder UseSwaggerDocs(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Always expose Swagger JSON for APIM import in all environments
        // This ensures /swagger/v1/swagger.json is available for Azure API Management
        app.UseSwagger(c =>
        {
            c.RouteTemplate = "swagger/{documentName}/swagger.json";
            c.SerializeAsV2 = false; // Use OpenAPI 3.0 format for better APIM compatibility
        });

        // Configure Swagger UI based on environment
        if (env.IsDevelopment())
        {
            // Full Swagger UI in Development for easy testing
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CloudGames Users API v1");
                c.RoutePrefix = "swagger";
                c.DocumentTitle = "CloudGames Users API Documentation";
                c.DefaultModelsExpandDepth(-1); // Hide models section by default
                c.DisplayRequestDuration();
            });
        }
        else
        {
            // In Production: Disable Swagger UI for security but keep JSON endpoint accessible
            // The /swagger/v1/swagger.json endpoint remains available for APIM import
            // Uncomment the lines below if you want to enable Swagger UI in production with basic auth
            /*
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CloudGames Users API v1");
                c.RoutePrefix = "api-docs"; // Use a different route for production
                c.DocumentTitle = "CloudGames Users API Documentation";
                c.DefaultModelsExpandDepth(-1);
                c.DisplayRequestDuration();
            });
            */
        }
        
        return app;
    }
}

