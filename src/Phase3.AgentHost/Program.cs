using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Phase3.AgentHost.Services;
using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Services.Authentication;
using Phase3.AzureDevOps.Services.Secrets;
using Phase3.AzureDevOps.Services.WorkItems;
using Phase3.AzureDevOps.Services.TestPlans;
using Phase3.AzureDevOps.Services.Git;
using Phase3.AzureDevOps.Services.Sync;
using Phase3.AzureDevOps.Services.Resilience;
using Phase3.AzureDevOps.Services.Observability;
using Phase3.AzureDevOps.Services.Performance;
using Phase3.AzureDevOps.Configuration;
using Serilog;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

namespace Phase3.AgentHost;

static class Program
{
    static async Task<int> Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.WithProperty("AgentName", configuration["Agent:Name"] ?? "CPU-Agent")
            .CreateLogger();

        try
        {
            Log.Information("Starting CPU Agent Host Application");
            Log.Information("Agent Name: {AgentName}", configuration["Agent:Name"]);
            Log.Information("Azure DevOps Organization: {OrgUrl}", configuration["AzureDevOps:OrganizationUrl"]);
            Log.Information("Azure DevOps Project: {Project}", configuration["AzureDevOps:ProjectName"]);

            var host = Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    // Register configuration sections
                    services.Configure<AgentConfiguration>(configuration.GetSection("Agent"));
                    services.Configure<ResilienceConfiguration>(configuration.GetSection("Resilience"));
                    services.Configure<TelemetryConfiguration>(configuration.GetSection("Telemetry"));
                    services.Configure<DPAPIConfiguration>(configuration.GetSection("Secrets:DPAPI"));

                    // Register Phase 3 services
                    RegisterPhase3Services(services, configuration);

                    // Register Agent services
                    services.AddSingleton<IWorkflowEngine, WorkflowEngine>();
                    services.AddSingleton<IWorkflowRepository, FileSystemWorkflowRepository>();
                    services.AddSingleton<IAIDecisionService, AIDecisionService>();
                    
                    // Register Agent Host as hosted service
                    services.AddHostedService<AgentHostService>();
                })
                .ConfigureOpenTelemetry(configuration)
                .Build();

            await host.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "CPU Agent Host Application terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void RegisterPhase3Services(IServiceCollection services, IConfiguration configuration)
    {
        // Authentication
        var authMethod = configuration["AzureDevOps:AuthenticationMethod"];
        if (authMethod == "PAT")
        {
            services.AddSingleton<IAuthenticationProvider>(sp =>
            {
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<PATAuthenticationProvider>>();
                return new PATAuthenticationProvider(
                    configuration["AzureDevOps:PAT"]!,
                    logger);
            });
        }
        // Note: MSAL and Certificate authentication providers can be added when implemented

        // Secrets Management
        var secretsProvider = configuration["Secrets:Provider"];
        if (secretsProvider == "AzureKeyVault")
        {
            services.AddSingleton<ISecretsProvider, AzureKeyVaultSecretsProvider>();
        }
        // Note: CredentialManager secrets provider can be added when implemented
        else
        {
            services.AddSingleton<ISecretsProvider, DPAPISecretsProvider>();
        }

        // Core Services
        services.AddSingleton<IWorkItemService, Phase3.AzureDevOps.Services.WorkItems.WorkItemService>();
        services.AddSingleton<IWorkItemCoordinator, Phase3.AzureDevOps.Services.Concurrency.WorkItemCoordinator>();
        services.AddSingleton<ITestPlanService, Phase3.AzureDevOps.Services.TestPlans.TestPlanService>();
        services.AddSingleton<IGitService, Phase3.AzureDevOps.Services.Git.GitService>();
        services.AddSingleton<IGitWorkspaceManager, Phase3.AzureDevOps.Services.Git.GitWorkspaceManager>();
        services.AddSingleton<IOfflineSyncService, Phase3.AzureDevOps.Services.Sync.OfflineSyncService>();

        // Resilience & Observability
        services.AddSingleton<IResiliencePolicy, Phase3.AzureDevOps.Services.Resilience.ResiliencePolicy>();
        services.AddSingleton<ITelemetryService, Phase3.AzureDevOps.Services.Observability.TelemetryService>();
        services.AddSingleton<ICacheService, Phase3.AzureDevOps.Services.Performance.CacheService>();
        services.AddSingleton<IRateLimiter, Phase3.AzureDevOps.Services.Performance.TokenBucketRateLimiter>();

        // Test Lifecycle & Migration
        services.AddSingleton<ITestCaseLifecycleManager, Phase3.AzureDevOps.Services.TestPlans.TestCaseLifecycleManager>();
        services.AddSingleton<IMigrationService, Phase3.AzureDevOps.Services.Migration.MigrationService>();
    }

    private static IHostBuilder ConfigureOpenTelemetry(this IHostBuilder builder, IConfiguration configuration)
    {
        var telemetryConfig = configuration.GetSection("Telemetry");
        var serviceName = telemetryConfig["ServiceName"] ?? "cpu-agent";
        var serviceVersion = telemetryConfig["ServiceVersion"] ?? "3.0.0";
        var otlpEndpoint = telemetryConfig["OtlpEndpoint"];

        if (!string.IsNullOrEmpty(otlpEndpoint))
        {
            builder.ConfigureServices(services =>
            {
                services.AddOpenTelemetry()
                    .ConfigureResource(resource => resource
                        .AddService(serviceName, serviceVersion: serviceVersion))
                    .WithTracing(tracing => tracing
                        .AddSource(serviceName)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint)))
                    .WithMetrics(metrics => metrics
                        .AddMeter(serviceName)
                        .AddRuntimeInstrumentation()
                        // Note: AddProcessInstrumentation not available in OpenTelemetry 1.7.0
                        .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint)));
            });
        }

        return builder;
    }
}

/// <summary>
/// Agent configuration model
/// </summary>
public class AgentConfiguration
{
    public string Name { get; set; } = "CPU-Agent";
    public int PollingIntervalSeconds { get; set; } = 60;
    public int MaxConcurrentWorkItems { get; set; } = 5;
    public string WorkItemQueryWiql { get; set; } = string.Empty;
    public string WorkflowsDirectory { get; set; } = "workflows";
}
