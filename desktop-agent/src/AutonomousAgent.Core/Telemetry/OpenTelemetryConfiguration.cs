using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AutonomousAgent.Core.Telemetry;

public static class OpenTelemetryConfiguration
{
    public static IHostApplicationBuilder AddOpenTelemetryInstrumentation(
        this IHostApplicationBuilder builder,
        string serviceName = "AutonomousAgent",
        string collectorEndpoint = "http://localhost:18418")
    {
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = builder.Environment.EnvironmentName,
                    ["host.name"] = Environment.MachineName
                }))
            .WithTracing(tracing => tracing
                .AddSource(serviceName)
                .AddSource("AutonomousAgent.*")
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithHttpRequestMessage = (activity, request) =>
                    {
                        activity.SetTag("http.user_agent", request.Headers.UserAgent?.ToString() ?? "unknown");
                        activity.SetTag("http.content_length", request.Content?.Headers.ContentLength ?? 0);
                    };
                })
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri(collectorEndpoint);
                    opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                }))
            .WithMetrics(metrics => metrics
                .AddMeter(serviceName)
                .AddMeter("Microsoft.AspNetCore.Hosting")
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                .AddMeter("System.Net.Http")
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri(collectorEndpoint);
                    opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                }));

        return builder;
    }
}