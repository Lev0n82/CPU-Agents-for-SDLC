namespace Phase3.IntegrationTests.Fixtures;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Services.Authentication;
using Phase3.AzureDevOps.Services.Concurrency;
using Phase3.AzureDevOps.Services.Git;
using Phase3.AzureDevOps.Services.Migration;
using Phase3.AzureDevOps.Services.Observability;
using Phase3.AzureDevOps.Services.Performance;
using Phase3.AzureDevOps.Services.Resilience;
using Phase3.AzureDevOps.Services.Secrets;
using Phase3.AzureDevOps.Services.Sync;
using Phase3.AzureDevOps.Services.TestPlans;
using Phase3.AzureDevOps.Services.WorkItems;
using Serilog;
using System;

/// <summary>
/// Integration test fixture that sets up dependency injection and services.
/// </summary>
public class IntegrationTestFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }
    public IConfiguration Configuration { get; }
    public string OrganizationUrl { get; }
    public string ProjectName { get; }

    public IntegrationTestFixture()
    {
        // Load configuration
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        OrganizationUrl = Configuration["AzureDevOps:OrganizationUrl"] 
            ?? throw new InvalidOperationException("OrganizationUrl not configured");
        ProjectName = Configuration["AzureDevOps:ProjectName"] 
            ?? throw new InvalidOperationException("ProjectName not configured");

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        // Build service collection
        var services = new ServiceCollection();
        ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Add configuration
        services.AddSingleton(Configuration);

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddSerilog(dispose: true);
        });

        // Add memory cache
        services.AddMemoryCache();

        // Phase 3.1 services (Critical Foundations)
        // Add PAT authentication
        var pat = Configuration["AzureDevOps:PAT"] ?? throw new InvalidOperationException("PAT not configured");
        services.AddSingleton<IAuthenticationProvider>(sp => 
            new PATAuthenticationProvider(pat, sp.GetRequiredService<ILogger<PATAuthenticationProvider>>()));
        services.AddSingleton<IWorkItemCoordinator, WorkItemCoordinator>();
        
        // Add DPAPI configuration
        services.AddSingleton(new Phase3.AzureDevOps.Configuration.DPAPIConfiguration
        {
            StoragePath = Path.Combine(Path.GetTempPath(), "cpu-agents-test-secrets")
        });
        services.AddSingleton<ISecretsProvider, DPAPISecretsProvider>();
        services.AddScoped<IWorkItemService, WorkItemService>();

        // Phase 3.2 services (Core Services)
        services.AddScoped<ITestPlanService, TestPlanService>();
        services.AddScoped<IGitService, GitService>();
        services.AddScoped<IOfflineSyncService, OfflineSyncService>();
        services.AddScoped<IGitWorkspaceManager, GitWorkspaceManager>();

        // Phase 3.3 services (Operational Resilience)
        services.AddSingleton<IResiliencePolicy, ResiliencePolicy>();
        services.AddSingleton<ITelemetryService, TelemetryService>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<IRateLimiter, TokenBucketRateLimiter>();

        // Phase 3.4 services (Migration & Testing)
        services.AddScoped<ITestCaseLifecycleManager, TestCaseLifecycleManager>();
        services.AddScoped<IMigrationService, MigrationService>();

        // Azure DevOps HTTP clients
        services.AddScoped(sp =>
        {
            var authProvider = sp.GetRequiredService<IAuthenticationProvider>();
            var token = authProvider.GetTokenAsync().Result;
            var credentials = new VssBasicCredential(string.Empty, token);
            var connection = new VssConnection(new Uri(OrganizationUrl), credentials);
            return connection.GetClient<WorkItemTrackingHttpClient>();
        });
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        Log.CloseAndFlush();
    }
}
