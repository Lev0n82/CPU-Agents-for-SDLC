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
using Phase3.AzureDevOps.Services.QA;
using Phase3.AzureDevOps.Configuration;
using Microsoft.EntityFrameworkCore;
using Serilog;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

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
                    services.Configure<KeyVaultConfiguration>(configuration.GetSection("Secrets:AzureKeyVault"));

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

            // Initialize SQLite database for QA cache
            using (var scope = host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<Phase3.AzureDevOps.Models.QA.QaCacheDbContext>();
                Log.Information("Ensuring QA cache database is created...");
                dbContext.Database.EnsureCreated();
                Log.Information("QA cache database initialized successfully");
            }

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
        // Secrets Management - Register first as other services depend on it
        var secretsProvider = configuration["Secrets:Provider"];
        if (secretsProvider == "AzureKeyVault")
        {
            // Register KeyVaultConfiguration directly (not as IOptions<>) since AzureKeyVaultSecretsProvider expects it directly
            var keyVaultConfig = new KeyVaultConfiguration
            {
                VaultUri = configuration["Secrets:AzureKeyVault:VaultUri"] ?? throw new InvalidOperationException("Secrets:AzureKeyVault:VaultUri is required")
            };
            services.AddSingleton(keyVaultConfig);
            services.AddSingleton<ISecretsProvider, AzureKeyVaultSecretsProvider>();
        }
        // Note: CredentialManager secrets provider can be added when implemented
        else
        {
            services.AddSingleton<ISecretsProvider, DPAPISecretsProvider>();
        }

        // Authentication
        var authMethod = configuration["AzureDevOps:AuthenticationMethod"];
        if (authMethod == "PAT")
        {
            services.AddSingleton<IAuthenticationProvider>(sp =>
            {
                var secretsProvider = sp.GetRequiredService<ISecretsProvider>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<PATAuthenticationProvider>>();
                var cacheToken = configuration.GetValue<bool>("AzureDevOps:CacheToken", true); // Default to true for lazy retrieval
                return new PATAuthenticationProvider(
                    secretsProvider,
                    logger,
                    cacheToken);
            });
        }
        // Note: MSAL and Certificate authentication providers can be added when implemented

        // Azure DevOps Clients - Register HTTP clients with authentication
        var orgUrl = configuration["AzureDevOps:OrganizationUrl"];
        var projectName = configuration["AzureDevOps:ProjectName"];
        
        services.AddSingleton<VssConnection>(sp =>
        {
            var authProvider = sp.GetRequiredService<IAuthenticationProvider>();
            var token = authProvider.GetTokenAsync().GetAwaiter().GetResult();
            var credentials = new VssBasicCredential(string.Empty, token);
            return new VssConnection(new Uri(orgUrl), credentials);
        });

        services.AddSingleton<WorkItemTrackingHttpClient>(sp =>
        {
            var connection = sp.GetRequiredService<VssConnection>();
            return connection.GetClient<WorkItemTrackingHttpClient>();
        });

        services.AddSingleton<TestManagementHttpClient>(sp =>
        {
            var connection = sp.GetRequiredService<VssConnection>();
            return connection.GetClient<TestManagementHttpClient>();
        });

        services.AddSingleton<GitHttpClient>(sp =>
        {
            var connection = sp.GetRequiredService<VssConnection>();
            return connection.GetClient<GitHttpClient>();
        });

        // Register WorkItemConfiguration
        services.AddSingleton(new WorkItemConfiguration
        {
            ProjectName = projectName ?? throw new InvalidOperationException("AzureDevOps:ProjectName is required")
        });

        // Register ConcurrencyConfiguration
        services.AddSingleton(new ConcurrencyConfiguration
        {
            ClaimDurationMinutes = 30,
            StaleClaimCheckIntervalMinutes = 5
        });

        // Register WIQLValidator
        services.AddSingleton<IWIQLValidator, Phase3.AzureDevOps.Services.WorkItems.WIQLValidator>();

        // Register Azure DevOps Client
        services.AddSingleton<IAzureDevOpsClient, Phase3.AzureDevOps.Services.Clients.AzureDevOpsClient>();

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
        
        // QA Dashboard Services
        services.AddSingleton<QADashboardService>();
        services.AddSingleton<QADataSyncService>();
        services.AddDbContext<Phase3.AzureDevOps.Models.QA.QaCacheDbContext>(options =>
        {
            options.UseSqlite("Data Source=qa-cache.db");
        });
        services.AddHostedService<QAMetricsExporterService>();
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
                        .AddMeter("Phase3.AzureDevOps.QAMetrics")  // QA metrics meter
                        .AddRuntimeInstrumentation()
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
