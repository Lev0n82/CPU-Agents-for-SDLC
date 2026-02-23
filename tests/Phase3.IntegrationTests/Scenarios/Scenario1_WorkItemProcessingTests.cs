namespace Phase3.IntegrationTests.Scenarios;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Phase3.AzureDevOps.Interfaces;
using Phase3.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// Scenario 1: End-to-End Work Item Processing
/// Tests the complete workflow of querying, claiming, updating, and tracking work items.
/// </summary>
[Collection("Integration Tests")]
public class Scenario1_WorkItemProcessingTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;
    private readonly ILogger<Scenario1_WorkItemProcessingTests> _logger;

    public Scenario1_WorkItemProcessingTests(
        IntegrationTestFixture fixture,
        ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _logger = _fixture.ServiceProvider.GetRequiredService<ILogger<Scenario1_WorkItemProcessingTests>>();
    }

    [Fact]
    public async Task Scenario1_CompleteWorkItemProcessingWorkflow()
    {
        // Arrange
        var workItemService = _fixture.ServiceProvider.GetRequiredService<IWorkItemService>();
        var workItemCoordinator = _fixture.ServiceProvider.GetRequiredService<IWorkItemCoordinator>();
        var cacheService = _fixture.ServiceProvider.GetRequiredService<ICacheService>();
        var telemetryService = _fixture.ServiceProvider.GetRequiredService<ITelemetryService>();
        var resiliencePolicy = _fixture.ServiceProvider.GetRequiredService<IResiliencePolicy>();

        _output.WriteLine("=== Scenario 1: End-to-End Work Item Processing ===");
        _output.WriteLine($"Organization: {_fixture.OrganizationUrl}");
        _output.WriteLine($"Project: {_fixture.ProjectName}");
        _output.WriteLine("");

        // Step 1: Authenticate with Azure DevOps
        _output.WriteLine("Step 1: Authenticating with Azure DevOps...");
        using var activity = telemetryService.StartActivity("Scenario1_WorkItemProcessing");
        
        // Step 2: Query available work items (with cache and resilience)
        _output.WriteLine("Step 2: Querying available work items...");
        var cacheKey = $"workitems:available:{_fixture.ProjectName}";
        var cachedWorkItems = cacheService.Get<List<int>>(cacheKey);
        
        List<int> workItemIds;
        if (cachedWorkItems != null)
        {
            _output.WriteLine($"  ✓ Cache hit: {cachedWorkItems.Count} work items");
            workItemIds = cachedWorkItems;
            telemetryService.IncrementCounter("cache.hits", 1);
        }
        else
        {
            _output.WriteLine("  ✓ Cache miss: Querying Azure DevOps");
            telemetryService.IncrementCounter("cache.misses", 1);
            
            var workItems = await resiliencePolicy.ExecuteAsync(async (ct) =>
            {
                return await workItemService.QueryWorkItemsAsync(
                    "SELECT [System.Id] FROM WorkItems WHERE [System.State] = 'New' ORDER BY [System.CreatedDate] DESC",
                    ct);
            });

            workItemIds = workItems.Select(wi => wi.Id.Value).ToList();
            cacheService.Set(cacheKey, workItemIds, TimeSpan.FromMinutes(5));
            _output.WriteLine($"  ✓ Found {workItemIds.Count} work items");
        }

        workItemIds.Should().NotBeEmpty("there should be work items available for testing");

        // Step 3: Claim a work item (with concurrency control)
        _output.WriteLine("Step 3: Claiming a work item...");
        var workItemId = workItemIds.First();
        var workItem = await workItemService.GetWorkItemAsync(workItemId);
        
        var claimResult = await workItemCoordinator.TryClaimWorkItemAsync(
            workItemId,
            workItem.Rev.Value,
            "IntegrationTest");

        claimResult.Should().BeTrue($"work item {workItemId} should be claimable");
        _output.WriteLine($"  ✓ Claimed work item {workItemId}");

        try
        {
            // Step 4: Get work item details
            _output.WriteLine("Step 4: Getting work item details...");
            workItem = await workItemService.GetWorkItemAsync(workItemId);
            
            workItem.Should().NotBeNull();
            workItem.Id.Should().Be(workItemId);
            
            var title = workItem.Fields.ContainsKey("System.Title") ? workItem.Fields["System.Title"]?.ToString() ?? "N/A" : "N/A";
            var state = workItem.Fields.ContainsKey("System.State") ? workItem.Fields["System.State"]?.ToString() ?? "N/A" : "N/A";
            
            _output.WriteLine($"  ✓ Work Item: {workItem.Id} - {title}");
            _output.WriteLine($"  ✓ State: {state}");

            // Step 5: Update work item state (with telemetry)
            _output.WriteLine("Step 5: Updating work item state...");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            var updateResult = await workItemService.UpdateWorkItemAsync(
                workItemId,
                workItem.Rev.Value,
                new Dictionary<string, object>
                {
                    ["System.State"] = "Active",
                    ["System.History"] = "Updated by Integration Test"
                });

            stopwatch.Stop();
            telemetryService.RecordHistogram("api_calls.duration", stopwatch.ElapsedMilliseconds);
            telemetryService.IncrementCounter("work_items.processed", 1);

            updateResult.Should().NotBeNull();
            _output.WriteLine($"  ✓ Updated work item state to Active");
            _output.WriteLine($"  ✓ API call duration: {stopwatch.ElapsedMilliseconds}ms");

            // Step 6: Verify metrics recorded
            _output.WriteLine("Step 6: Verifying metrics...");
            var cacheStats = cacheService.GetStatistics();
            _output.WriteLine($"  ✓ Cache Statistics:");
            _output.WriteLine($"    - Hits: {cacheStats.Hits}");
            _output.WriteLine($"    - Misses: {cacheStats.Misses}");
            _output.WriteLine($"    - Hit Rate: {cacheStats.HitRate:P2}");

            // Step 7: Check circuit breaker state
            _output.WriteLine("Step 7: Checking circuit breaker state...");
            var circuitState = resiliencePolicy.GetCircuitState();
            _output.WriteLine($"  ✓ Circuit Breaker State: {circuitState}");
            circuitState.Should().Be(Phase3.AzureDevOps.Interfaces.CircuitState.Closed, "circuit should be closed for successful operations");
        }
        finally
        {
            // Step 8: Release work item claim
            _output.WriteLine("Step 8: Releasing work item claim...");
            var latestWorkItem = await workItemService.GetWorkItemAsync(workItemId);
            await workItemCoordinator.ReleaseWorkItemAsync(workItemId, latestWorkItem.Rev.Value, "IntegrationTest");
            _output.WriteLine($"  ✓ Released work item {workItemId}");
        }

        _output.WriteLine("");
        _output.WriteLine("=== Scenario 1 Complete ===");
        _output.WriteLine("✓ All steps passed successfully");
    }

    [Fact]
    public async Task Scenario1_CacheEffectiveness()
    {
        // Test cache hit rate improvement
        var workItemService = _fixture.ServiceProvider.GetRequiredService<IWorkItemService>();
        var cacheService = _fixture.ServiceProvider.GetRequiredService<ICacheService>();

        _output.WriteLine("=== Testing Cache Effectiveness ===");

        // First call - cache miss
        var workItemId = 1;
        var cacheKey = $"workitem:{workItemId}";
        
        var cached = cacheService.Get<object>(cacheKey);
        cached.Should().BeNull("first call should be cache miss");

        // Simulate caching
        cacheService.Set(cacheKey, new { Id = workItemId, Title = "Test" }, TimeSpan.FromMinutes(5));

        // Second call - cache hit
        cached = cacheService.Get<object>(cacheKey);
        cached.Should().NotBeNull("second call should be cache hit");

        var stats = cacheService.GetStatistics();
        _output.WriteLine($"Cache Hit Rate: {stats.HitRate:P2}");
        stats.HitRate.Should().BeGreaterThan(0, "cache should have hits");
    }
}
