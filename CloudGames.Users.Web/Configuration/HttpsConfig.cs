namespace CloudGames.Users.Web.Configuration;

public static class HttpsConfig
{
    public static IApplicationBuilder UseHttpsSupport(this IApplicationBuilder app, IWebHostEnvironment environment)
    {
        // Add HTTPS redirection and HSTS as optional, without impacting local execution
        if (environment.IsProduction())
        {
            // HSTS for production only (recommended by Microsoft)
            app.UseHsts();
        }

        // HTTPS redirection (optional - can be disabled if APIM handles TLS termination)
        // Uncomment the line below if you want to force HTTPS redirection
        // app.UseHttpsRedirection();

        return app;
    }
}
