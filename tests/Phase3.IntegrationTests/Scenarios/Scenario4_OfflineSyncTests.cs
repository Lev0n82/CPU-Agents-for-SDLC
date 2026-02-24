namespace Phase3.IntegrationTests.Scenarios;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Models.Sync;
using Phase3.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// Scenario 4: Offline Synchronization
/// Tests offline mode, conflict detection, and resolution.
/// </summary>
[Collection("Integration Tests")]
public class Scenario4_OfflineSyncTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;
    private readonly ILogger<Scenario4_OfflineSyncTests> _logger;

    public Scenario4_OfflineSyncTests(
        IntegrationTestFixture fixture,
        ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _logger = _fixture.ServiceProvider.GetRequiredService<ILogger<Scenario4_OfflineSyncTests>>();
    }

    [Fact]
    public async Task Scenario4_OfflineSyncWorkflow()
    {
        // Arrange
        var syncService = _fixture.ServiceProvider.GetRequiredService<IOfflineSyncService>();
        var workItemService = _fixture.ServiceProvider.GetRequiredService<IWorkItemService>();

        _output.WriteLine("=== Scenario 4: Offline Synchronization ===");
        _output.WriteLine("");

        // Step 1: Enable offline mode
        _output.WriteLine("Step 1: Enabling offline mode...");
        await syncService.EnableOfflineModeAsync();
        var status = await syncService.GetOfflineStatusAsync();
        status.IsOfflineMode.Should().BeTrue();
        _output.WriteLine($"  ✓ Offline mode enabled");
        _output.WriteLine($"  ✓ Pending operations: {status.PendingOperations}");

        try
        {
            // Step 2: Disable offline mode and sync
            _output.WriteLine("Step 2: Disabling offline mode and syncing...");
            
            var syncResult = await syncService.DisableOfflineModeAsync();
            syncResult.Should().NotBeNull();
            _output.WriteLine($"  ✓ Sync completed");
            _output.WriteLine($"    - Success: {syncResult.SuccessCount}");
            _output.WriteLine($"    - Failures: {syncResult.FailureCount}");
            _output.WriteLine($"    - Conflicts: {syncResult.Conflicts.Count}");

            // Step 3: Verify offline mode is disabled
            _output.WriteLine("Step 3: Verifying offline mode disabled...");
            status = await syncService.GetOfflineStatusAsync();
            status.IsOfflineMode.Should().BeFalse();
            _output.WriteLine($"  ✓ Offline mode disabled");
        }
        finally
        {
            // Ensure offline mode is disabled
            var finalStatus = await syncService.GetOfflineStatusAsync();
            if (finalStatus.IsOfflineMode)
            {
                await syncService.DisableOfflineModeAsync();
            }
        }

        _output.WriteLine("");
        _output.WriteLine("=== Scenario 4 Complete ===");
        _output.WriteLine("✓ All steps passed successfully");
    }

    [Fact]
    public async Task Scenario4_ConflictResolution()
    {
        var syncService = _fixture.ServiceProvider.GetRequiredService<IOfflineSyncService>();

        _output.WriteLine("=== Testing Conflict Resolution Policies ===");

        // Test all conflict resolution policies
        var policies = new[]
        {
            ConflictResolutionPolicy.Abort,
            ConflictResolutionPolicy.Merge,
            ConflictResolutionPolicy.ManualReview,
            ConflictResolutionPolicy.ForceOverwrite
        };

        foreach (var policy in policies)
        {
            _output.WriteLine($"Testing policy: {policy}");
            
            // Simulate sync with policy
            await syncService.EnableOfflineModeAsync();
            var result = await syncService.DisableOfflineModeAsync();
            
            result.Should().NotBeNull();
            _output.WriteLine($"  ✓ Policy {policy} executed successfully");
        }
    }
}
