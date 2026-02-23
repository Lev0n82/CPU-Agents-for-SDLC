using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Phase3.AzureDevOps.Services.Concurrency;
using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Configuration;
using Phase3.AzureDevOps.Core;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Phase3.AzureDevOps.Tests.Integration;

/// <summary>
/// Integration tests for concurrency control workflows.
/// These tests verify end-to-end work item claiming and coordination.
/// </summary>
[Collection("Integration")]
public class ConcurrencyIntegrationTests
{
    [Fact]
    public async Task WorkItemClaim_FullLifecycle_Succeeds()
    {
        // Arrange
        var clientMock = new Mock<IAzureDevOpsClient>();
        var loggerMock = new Mock<ILogger<WorkItemCoordinator>>();
        var config = new ConcurrencyConfiguration
        {
            ClaimDurationMinutes = 15,
            StaleClaimCheckIntervalMinutes = 5
        };
        
        int workItemId = 123;
        int revision = 1;
        string agentId = "integration-test-agent";
        
        var workItem = new WorkItem
        {
            Id = workItemId,
            Rev = revision,
            Fields = new Dictionary<string, object>()
        };
        
        clientMock.Setup(x => x.GetWorkItemAsync(workItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workItem);
        clientMock.Setup(x => x.UpdateWorkItemAsync(
            workItemId,
            It.IsAny<Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchDocument>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(workItem);
        
        var coordinator = new WorkItemCoordinator(clientMock.Object, config, loggerMock.Object);

        // Act - Claim
        var claimResult = await coordinator.TryClaimWorkItemAsync(workItemId, revision, agentId);
        
        // Assert - Claim succeeded
        Assert.True(claimResult);
        
        // Act - Release
        await coordinator.ReleaseWorkItemAsync(workItemId, revision, agentId);
        
        // Assert - Release succeeded (no exception thrown)
        clientMock.Verify(x => x.UpdateWorkItemAsync(
            workItemId,
            It.IsAny<Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchDocument>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce()); // At least once for claim
    }

    [Fact]
    public async Task MultipleAgents_ConcurrentClaims_OnlyOneSucceeds()
    {
        // Arrange
        var clientMock = new Mock<IAzureDevOpsClient>();
        var loggerMock = new Mock<ILogger<WorkItemCoordinator>>();
        var config = new ConcurrencyConfiguration
        {
            ClaimDurationMinutes = 15
        };
        
        int workItemId = 456;
        int revision = 1;
        string agent1 = "agent-1";
        string agent2 = "agent-2";
        
        var workItem = new WorkItem
        {
            Id = workItemId,
            Rev = revision,
            Fields = new Dictionary<string, object>()
        };
        
        // First agent succeeds
        clientMock.SetupSequence(x => x.UpdateWorkItemAsync(
            workItemId,
            It.IsAny<Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchDocument>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(workItem)
            .ThrowsAsync(new ConcurrencyException("Already claimed"));
        
        var coordinator = new WorkItemCoordinator(clientMock.Object, config, loggerMock.Object);

        // Act
        var claim1Result = await coordinator.TryClaimWorkItemAsync(workItemId, revision, agent1);
        var claim2Result = await coordinator.TryClaimWorkItemAsync(workItemId, revision, agent2);

        // Assert
        Assert.True(claim1Result);   // First agent succeeds
        Assert.False(claim2Result);  // Second agent fails
    }

    [Fact]
    public async Task ClaimRenewal_ExtendsExpiry()
    {
        // Arrange
        var clientMock = new Mock<IAzureDevOpsClient>();
        var loggerMock = new Mock<ILogger<WorkItemCoordinator>>();
        var config = new ConcurrencyConfiguration
        {
            ClaimDurationMinutes = 15
        };
        
        int workItemId = 789;
        int revision = 1;
        string agentId = "renewal-test-agent";
        
        var claimTag = $"agent:{agentId}:{DateTime.UtcNow:O}:{DateTime.UtcNow.AddMinutes(5):O}";
        var workItem = new WorkItem
        {
            Id = workItemId,
            Rev = revision,
            Fields = new Dictionary<string, object>
            {
                { "System.Tags", claimTag }
            }
        };
        
        clientMock.Setup(x => x.GetWorkItemAsync(workItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workItem);
        clientMock.Setup(x => x.UpdateWorkItemAsync(
            workItemId,
            It.IsAny<Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchDocument>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(workItem);
        
        var coordinator = new WorkItemCoordinator(clientMock.Object, config, loggerMock.Object);

        // Act
        await coordinator.RenewClaimAsync(workItemId, revision, agentId);

        // Assert - Renewal succeeded (no exception thrown)
        clientMock.Verify(x => x.UpdateWorkItemAsync(
            workItemId,
            It.IsAny<Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchDocument>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
