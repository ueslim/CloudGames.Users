using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace CloudGames.Users.Web.Configuration;

public static class ObservabilityConfig
{
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        var serviceName = builder.Configuration["Service:Name"] ?? "CloudGames.Users";

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("service.name", serviceName)
            .WriteTo.Console()
            .CreateLogger();
        builder.Host.UseSerilog();

        var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
        var enableOtlp = !string.IsNullOrWhiteSpace(otlpEndpoint);

        var telemetryBuilder = builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName, serviceVersion: "1.0.0"))
            .WithTracing(t =>
            {
                t.AddAspNetCoreInstrumentation()
                 .AddHttpClientInstrumentation();
                
                if (enableOtlp)
                {
                    t.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint!));
                }
            })
            .WithMetrics(m =>
            {
                m.AddAspNetCoreInstrumentation()
                 .AddHttpClientInstrumentation();
                
                if (enableOtlp)
                {
                    m.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint!));
                }
            });

        if (!enableOtlp)
        {
            Log.Information("OpenTelemetry OTLP exporter is disabled. Set 'OpenTelemetry:OtlpEndpoint' in configuration to enable.");
        }

        return builder;
    }

    public static IApplicationBuilder UseCorrelation(this IApplicationBuilder app)
    {
        app.Use(async (ctx, next) =>
        {
            if (!ctx.Request.Headers.TryGetValue("x-correlation-id", out var corr))
            {
                corr = Guid.NewGuid().ToString();
                ctx.Response.Headers["x-correlation-id"] = corr.ToString();
            }
            Serilog.Context.LogContext.PushProperty("correlation_id", corr.ToString());
            var userId = ctx.User?.FindFirst("sub")?.Value ?? ctx.User?.Identity?.Name ?? string.Empty;
            if (!string.IsNullOrEmpty(userId))
            {
                Serilog.Context.LogContext.PushProperty("user_id", userId);
            }
            await next();
        });
        return app;
    }
}

