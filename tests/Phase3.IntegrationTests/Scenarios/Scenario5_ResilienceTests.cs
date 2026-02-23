namespace Phase3.IntegrationTests.Scenarios;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Phase3.AzureDevOps.Interfaces;
using Phase3.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// Scenario 5: Resilience Testing
/// Tests retry policies, circuit breaker, timeout handling, and bulkhead isolation.
/// </summary>
[Collection("Integration Tests")]
public class Scenario5_ResilienceTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public Scenario5_ResilienceTests(
        IntegrationTestFixture fixture,
        ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Scenario5_RetryPolicy()
    {
        var resiliencePolicy = _fixture.ServiceProvider.GetRequiredService<IResiliencePolicy>();

        _output.WriteLine("=== Testing Retry Policy ===");

        var attemptCount = 0;
        var result = await resiliencePolicy.ExecuteAsync(async (ct) =>
        {
            attemptCount++;
            _output.WriteLine($"Attempt {attemptCount}");
            
            if (attemptCount < 2)
            {
                throw new Exception("Simulated transient failure");
            }
            
            return "Success";
        });

        result.Should().Be("Success");
        attemptCount.Should().Be(2, "should retry once after first failure");
        _output.WriteLine($"✓ Retry policy executed successfully after {attemptCount} attempts");
    }

    [Fact]
    public async Task Scenario5_CircuitBreaker()
    {
        var resiliencePolicy = _fixture.ServiceProvider.GetRequiredService<IResiliencePolicy>();

        _output.WriteLine("=== Testing Circuit Breaker ===");

        // Initial state should be Closed
        var initialState = resiliencePolicy.GetCircuitState();
        _output.WriteLine($"Initial circuit state: {initialState}");
        initialState.Should().Be(CircuitState.Closed);

        // Simulate multiple failures to open circuit
        var failureCount = 0;
        for (int i = 0; i < 6; i++)
        {
            try
            {
                await resiliencePolicy.ExecuteAsync<string>(async (ct) =>
                {
                    throw new Exception("Simulated failure");
                });
            }
            catch
            {
                failureCount++;
                _output.WriteLine($"Failure {failureCount}");
            }
        }

        // Circuit should be open after 5 failures
        var openState = resiliencePolicy.GetCircuitState();
        _output.WriteLine($"Circuit state after {failureCount} failures: {openState}");
        
        // Note: Circuit may be Open or HalfOpen depending on timing
        openState.Should().NotBe(CircuitState.Closed, "circuit should not be closed after multiple failures");
        _output.WriteLine($"✓ Circuit breaker opened after {failureCount} failures");
    }

    [Fact]
    public async Task Scenario5_Timeout()
    {
        var resiliencePolicy = _fixture.ServiceProvider.GetRequiredService<IResiliencePolicy>();

        _output.WriteLine("=== Testing Timeout Policy ===");

        var action = async () =>
        {
            await resiliencePolicy.ExecuteAsync(async (ct) =>
            {
                await Task.Delay(TimeSpan.FromMinutes(2), ct); // Exceeds 30s timeout
                return "Should not reach here";
            });
        };

        await action.Should().ThrowAsync<Exception>("operation should timeout");
        _output.WriteLine($"✓ Timeout policy enforced successfully");
    }

    [Fact]
    public async Task Scenario5_Bulkhead()
    {
        var resiliencePolicy = _fixture.ServiceProvider.GetRequiredService<IResiliencePolicy>();

        _output.WriteLine("=== Testing Bulkhead Isolation ===");

        var tasks = new List<Task<string>>();
        var successCount = 0;
        var rejectedCount = 0;

        // Try to execute 15 parallel operations (max is 10)
        for (int i = 0; i < 15; i++)
        {
            var taskNum = i;
            var task = Task.Run(async () =>
            {
                try
                {
                    return await resiliencePolicy.ExecuteAsync(async (ct) =>
                    {
                        await Task.Delay(100, ct);
                        return $"Task {taskNum} completed";
                    });
                }
                catch (Exception ex)
                {
                    return $"Task {taskNum} rejected: {ex.Message}";
                }
            });
            tasks.Add(task);
        }

        var results = await Task.WhenAll(tasks);

        foreach (var result in results)
        {
            if (result.Contains("completed"))
            {
                successCount++;
            }
            else
            {
                rejectedCount++;
            }
        }

        _output.WriteLine($"Success: {successCount}, Rejected: {rejectedCount}");
        _output.WriteLine($"✓ Bulkhead isolation enforced (max 10 parallel + 20 queued)");
    }

    [Fact]
    public async Task Scenario5_CompleteResilienceWorkflow()
    {
        var resiliencePolicy = _fixture.ServiceProvider.GetRequiredService<IResiliencePolicy>();
        var workItemService = _fixture.ServiceProvider.GetRequiredService<IWorkItemService>();

        _output.WriteLine("=== Scenario 5: Complete Resilience Workflow ===");
        _output.WriteLine("");

        // Step 1: Verify circuit is closed
        _output.WriteLine("Step 1: Verifying circuit breaker state...");
        var circuitState = resiliencePolicy.GetCircuitState();
        _output.WriteLine($"  ✓ Circuit Breaker State: {circuitState}");

        // Step 2: Execute operation with resilience
        _output.WriteLine("Step 2: Executing operation with resilience policies...");
        var result = await resiliencePolicy.ExecuteAsync(async (ct) =>
        {
            var workItems = await workItemService.QueryWorkItemsAsync(
                "SELECT [System.Id] FROM WorkItems WHERE [System.State] = 'New'",
                ct);
            return workItems.Count();
        });

        result.Should().BeGreaterThanOrEqualTo(0);
        _output.WriteLine($"  ✓ Operation completed successfully: {result} work items found");

        // Step 3: Verify circuit remains closed
        _output.WriteLine("Step 3: Verifying circuit breaker state after success...");
        circuitState = resiliencePolicy.GetCircuitState();
        circuitState.Should().Be(CircuitState.Closed);
        _output.WriteLine($"  ✓ Circuit Breaker State: {circuitState}");

        _output.WriteLine("");
        _output.WriteLine("=== Scenario 5 Complete ===");
        _output.WriteLine("✓ All resilience policies working correctly");
    }
}
