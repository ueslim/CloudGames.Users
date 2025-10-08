namespace CloudGames.Users.Web.Configuration;

public static class CorsConfig
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IWebHostEnvironment environment)
    {
        // Only add CORS for local/dev environment - let APIM handle CORS in production
        if (environment.IsDevelopment() || environment.IsStaging())
        {
            services.AddCors(options =>
            {
                options.AddPolicy("LocalDevelopment", policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:4200",  // Angular development server
                            "https://localhost:4200"  // Angular with HTTPS
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
        }

        return services;
    }

    public static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder app, IWebHostEnvironment environment)
    {
        // Only use CORS for local/dev environment
        if (environment.IsDevelopment() || environment.IsStaging())
        {
            app.UseCors("LocalDevelopment");
        }

        return app;
    }
}
