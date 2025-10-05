using Microsoft.AspNetCore.Mvc;

namespace CloudGames.Users.Web.Configuration;

public static class ApiConfig
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = false;
        });
        services.AddEndpointsApiExplorer();
        return services;
    }

    public static IApplicationBuilder UseApi(this IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/health", () => Results.Ok("ok"));
        });
        return app;
    }
}

