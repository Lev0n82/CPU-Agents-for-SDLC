using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Phase3.AzureDevOps.Telemetry;

public static class OpenTelemetryConfiguration
{
    public static IServiceCollection AddOpenTelemetryInstrumentation(
        this IServiceCollection services,
        string serviceName,
        string collectorEndpoint)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName))
            .WithTracing(tracing => tracing
                .AddSource(serviceName)
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri(collectorEndpoint);
                }))
            .WithMetrics(metrics => metrics
                .AddMeter(serviceName)
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri(collectorEndpoint);
                }));

        return services;
    }
}