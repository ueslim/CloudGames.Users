using System.Net;
using System.Text.Json;

namespace CloudGames.Users.Web.Configuration;

public static class ExceptionHandlingConfig
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.ContentType = "application/problem+json";
                
                var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
                
                // Map exceptions to appropriate HTTP status codes and messages
                // Note: ArgumentNullException must come before ArgumentException since it inherits from it
                var (statusCode, title) = exception switch
                {
                    ArgumentNullException => (HttpStatusCode.BadRequest, "Bad Request"),
                    ArgumentException => (HttpStatusCode.BadRequest, "Bad Request"),
                    KeyNotFoundException => (HttpStatusCode.NotFound, "Not Found"),
                    InvalidOperationException => (HttpStatusCode.Conflict, "Conflict"),
                    UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized"),
                    _ => (HttpStatusCode.InternalServerError, "Internal Server Error")
                };

                context.Response.StatusCode = (int)statusCode;
                
                // Create consistent error response
                var errorResponse = new
                {
                    type = $"https://httpstatus.es/{(int)statusCode}",
                    title = title,
                    status = (int)statusCode,
                    detail = exception?.Message ?? title,
                    instance = context.Request.Path,
                    timestamp = DateTime.UtcNow
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
            });
        });

        return app;
    }
}
