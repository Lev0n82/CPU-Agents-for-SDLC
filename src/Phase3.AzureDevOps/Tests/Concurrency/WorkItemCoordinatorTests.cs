using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Phase3.AzureDevOps.Services.Concurrency;
using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Models;
using Phase3.AzureDevOps.Configuration;
using Phase3.AzureDevOps.Core;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Phase3.AzureDevOps.Tests.Concurrency;

public class WorkItemCoordinatorTests
{
    private readonly Mock<IAzureDevOpsClient> _clientMock;
    private readonly Mock<ILogger<WorkItemCoordinator>> _loggerMock;
    private readonly ConcurrencyConfiguration _config;
    private readonly WorkItemCoordinator _coordinator;

    public WorkItemCoordinatorTests()
    {
        _clientMock = new Mock<IAzureDevOpsClient>();
        _loggerMock = new Mock<ILogger<WorkItemCoordinator>>();
        _config = new ConcurrencyConfiguration 
        { 
            ClaimDurationMinutes = 15,
            StaleClaimCheckIntervalMinutes = 5
        };
        _coordinator = new WorkItemCoordinator(_clientMock.Object, _config, _loggerMock.Object);
    }

    [Fact]
    public async Task ClaimWorkItemAsync_AvailableWorkItem_SuccessfullyClaims()
    {
        // Arrange
        int workItemId = 123;
        string agentId = "agent-1";
        var workItem = new WorkItem { Id = workItemId, Rev = 1, Fields = new Dictionary<string, object>() };
        
        _clientMock.Setup(x => x.GetWorkItemAsync(workItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workItem);
        _clientMock.Setup(x => x.UpdateWorkItemAsync(workItemId, It.IsAny<Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchDocument>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workItem);

        // Act
        var result = await _coordinator.ClaimWorkItemAsync(workItemId, agentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(workItemId, result.WorkItemId);
        Assert.Equal(agentId, result.AgentId);
    }

    [Fact]
    public async Task ClaimWorkItemAsync_AlreadyClaimed_ThrowsConcurrencyException()
    {
        // Arrange
        int workItemId = 123;
        string agentId = "agent-1";
        var workItem = new WorkItem 
        { 
            Id = workItemId, 
            Rev = 1, 
            Fields = new Dictionary<string, object>
            {
                { "Custom.ProcessingAgent", "other-agent" },
                { "Custom.ClaimExpiry", DateTime.UtcNow.AddMinutes(10) }
            }
        };
        
        _clientMock.Setup(x => x.GetWorkItemAsync(workItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workItem);

        // Act & Assert
        await Assert.ThrowsAsync<ConcurrencyException>(() => 
            _coordinator.ClaimWorkItemAsync(workItemId, agentId));
    }

    [Fact]
    public async Task ReleaseWorkItemAsync_OwnedWorkItem_SuccessfullyReleases()
    {
        // Arrange
        int workItemId = 123;
        int revision = 2;
        string agentId = "agent-1";
        var workItem = new WorkItem 
        { 
            Id = workItemId, 
            Rev = revision, 
            Fields = new Dictionary<string, object>
            {
                { "Custom.ProcessingAgent", agentId }
            }
        };
        
        _clientMock.Setup(x => x.GetWorkItemAsync(workItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workItem);
        _clientMock.Setup(x => x.UpdateWorkItemAsync(workItemId, It.IsAny<Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchDocument>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workItem);

        // Act
        await _coordinator.ReleaseWorkItemAsync(workItemId, revision, agentId);

        // Assert
        _clientMock.Verify(x => x.UpdateWorkItemAsync(
            workItemId, 
            It.IsAny<Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchDocument>(), 
            It.IsAny<int?>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RenewClaimAsync_OwnedWorkItem_SuccessfullyRenews()
    {
        // Arrange
        int workItemId = 123;
        string agentId = "agent-1";
        var workItem = new WorkItem 
        { 
            Id = workItemId, 
            Rev = 2, 
            Fields = new Dictionary<string, object>
            {
                { "Custom.ProcessingAgent", agentId },
                { "Custom.ClaimExpiry", DateTime.UtcNow.AddMinutes(5) }
            }
        };
        
        _clientMock.Setup(x => x.GetWorkItemAsync(workItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workItem);
        _clientMock.Setup(x => x.UpdateWorkItemAsync(workItemId, It.IsAny<Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchDocument>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workItem);

        // Act
        var result = await _coordinator.RenewClaimAsync(workItemId, agentId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ExpiresAt > DateTime.UtcNow.AddMinutes(10));
    }
}
