namespace Phase3.IntegrationTests.Scenarios;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Models.Git;
using Phase3.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// Scenario 3: Git Integration with Workspace Management
/// Tests Git operations, workspace management, and rate limiting.
/// </summary>
[Collection("Integration Tests")]
public class Scenario3_GitIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;
    private readonly ILogger<Scenario3_GitIntegrationTests> _logger;

    public Scenario3_GitIntegrationTests(
        IntegrationTestFixture fixture,
        ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _logger = _fixture.ServiceProvider.GetRequiredService<ILogger<Scenario3_GitIntegrationTests>>();
    }

    [Fact]
    public async Task Scenario3_CompleteGitWorkflow()
    {
        // Arrange
        var gitService = _fixture.ServiceProvider.GetRequiredService<IGitService>();
        var workspaceManager = _fixture.ServiceProvider.GetRequiredService<IGitWorkspaceManager>();
        var rateLimiter = _fixture.ServiceProvider.GetRequiredService<IRateLimiter>();

        _output.WriteLine("=== Scenario 3: Git Integration ===");

        // Step 1: Create workspace
        _output.WriteLine("Step 1: Creating Git workspace...");
        var workspaceRequest = new CreateWorkspaceRequest
        {
            Name = $"integration-test-{Guid.NewGuid():N}",
            RepositoryUrl = "https://github.com/microsoft/vscode.git",
            ShallowClone = true
        };

        var workspace = await workspaceManager.CreateWorkspaceAsync(workspaceRequest);
        workspace.Should().NotBeNull();
        workspace.Path.Should().NotBeNullOrEmpty();
        _output.WriteLine($"  ✓ Created workspace: {workspace.Path}");

        try
        {
            // Step 2: Clone repository (with rate limiting)
            _output.WriteLine("Step 2: Cloning repository...");
            
            // Check rate limiter
            var tokensAvailable = await rateLimiter.TryAcquireAsync();
            tokensAvailable.Should().BeTrue("rate limiter should allow request");
            _output.WriteLine($"  ✓ Rate limiter: Token acquired");

            var cloneRequest = new CloneRepositoryRequest
            {
                RepositoryUrl = "https://github.com/microsoft/vscode.git",
                LocalPath = workspace.Path,
                ShallowClone = true
            };

            await gitService.CloneRepositoryAsync(cloneRequest);

            Directory.Exists(workspace.Path).Should().BeTrue();
            _output.WriteLine($"  ✓ Repository cloned to: {workspace.Path}");

            // Step 3: Commit changes
            _output.WriteLine("Step 3: Committing changes...");
            var testFilePath = Path.Combine(workspace.Path, "test-file.txt");
            await File.WriteAllTextAsync(testFilePath, "Integration test content");

            var commitRequest = new CommitChangesRequest
            {
                RepositoryPath = workspace.Path,
                CommitMessage = "Integration test commit",
                AuthorName = "Integration Test",
                AuthorEmail = "test@example.com"
            };

            var commitId = await gitService.CommitChangesAsync(commitRequest);
            commitId.Should().NotBeNullOrEmpty();
            _output.WriteLine($"  ✓ Committed changes: {commitId}");

            // Step 4: Verify workspace cached
            _output.WriteLine("Step 4: Verifying workspace is cached...");
            var cachedWorkspace = await workspaceManager.GetWorkspaceAsync(workspace.Id);
            cachedWorkspace.Should().NotBeNull();
            cachedWorkspace!.Path.Should().Be(workspace.Path);
            _output.WriteLine($"  ✓ Workspace cached and retrieved");

            // Step 5: Get workspace statistics
            _output.WriteLine("Step 5: Getting workspace statistics...");
            var stats = await workspaceManager.GetStatisticsAsync();
            stats.Should().NotBeNull();
            _output.WriteLine($"  ✓ Workspace Statistics:");
            _output.WriteLine($"    - Total Workspaces: {stats.TotalWorkspaces}");
            _output.WriteLine($"    - Total Size: {stats.TotalDiskSpaceBytes / 1024 / 1024:F2} MB");
            _output.WriteLine($"    - Disk Usage: {stats.DiskUsagePercent:F2}%");
        }
        finally
        {
            // Step 6: Delete workspace
            _output.WriteLine("Step 6: Deleting workspace...");
            await workspaceManager.CleanupWorkspaceAsync(workspace.Id);
            _output.WriteLine($"  ✓ Workspace deleted");
        }

        _output.WriteLine("");
        _output.WriteLine("=== Scenario 3 Complete ===");
        _output.WriteLine("✓ All steps passed successfully");
    }

    [Fact]
    public async Task Scenario3_RateLimiterEffectiveness()
    {
        var rateLimiter = _fixture.ServiceProvider.GetRequiredService<IRateLimiter>();

        _output.WriteLine("=== Testing Rate Limiter ===");

        // Acquire tokens rapidly
        var successCount = 0;
        var failureCount = 0;

        for (int i = 0; i < 30; i++)
        {
            if (await rateLimiter.TryAcquireAsync())
            {
                successCount++;
            }
            else
            {
                failureCount++;
            }
        }

        _output.WriteLine($"Success: {successCount}, Failures: {failureCount}");
        failureCount.Should().BeGreaterThan(0, "rate limiter should reject some requests");
    }
}
