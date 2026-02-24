namespace Phase3.AzureDevOps.Tests.Git;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Phase3.AzureDevOps.Exceptions;
using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Models.Git;
using Phase3.AzureDevOps.Services.Git;
using Xunit;

/// <summary>
/// Unit tests for GitWorkspaceManager.
/// </summary>
public class GitWorkspaceManagerTests
{
    private readonly Mock<IGitService> _mockGitService;
    private readonly Mock<ILogger<GitWorkspaceManager>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly string _testWorkspaceRoot;

    public GitWorkspaceManagerTests()
    {
        _mockGitService = new Mock<IGitService>();
        _mockLogger = new Mock<ILogger<GitWorkspaceManager>>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        // Use unique test workspace root
        _testWorkspaceRoot = Path.Combine(Path.GetTempPath(), $"test_workspaces_{Guid.NewGuid()}");
        _mockConfiguration.Setup(c => c["GitWorkspace:RootPath"]).Returns(_testWorkspaceRoot);
        _mockConfiguration.Setup(c => c.GetValue<long>("GitWorkspace:MaxDiskUsageGB", 100)).Returns(100);
        _mockConfiguration.Setup(c => c.GetValue<double>("GitWorkspace:CleanupThresholdPercent", 90)).Returns(90.0);
    }

    [Fact]
    public void CreateWorkspaceAsync_EmptyName_ThrowsValidationException()
    {
        // Arrange
        var service = CreateService();
        var request = new CreateWorkspaceRequest
        {
            Name = "" // Invalid
        };

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => service.CreateWorkspaceAsync(request));
    }

    [Fact]
    public async Task CreateWorkspaceAsync_ValidRequest_CreatesWorkspace()
    {
        // Arrange
        var service = CreateService();
        var request = new CreateWorkspaceRequest
        {
            Name = "test-workspace-1"
        };

        // Act
        var workspace = await service.CreateWorkspaceAsync(request);

        // Assert
        Assert.NotNull(workspace);
        Assert.Equal("test-workspace-1", workspace.Name);
        Assert.NotEqual(Guid.Empty, workspace.Id);
        Assert.True(Directory.Exists(workspace.Path));
    }

    [Fact]
    public async Task GetWorkspaceAsync_NonExistentWorkspace_ThrowsNotFoundException()
    {
        // Arrange
        var service = CreateService();
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetWorkspaceAsync(nonExistentId));
    }

    [Fact]
    public async Task ListWorkspacesAsync_NoWorkspaces_ReturnsEmptyList()
    {
        // Arrange
        var service = CreateService();

        // Act
        var workspaces = await service.ListWorkspacesAsync();

        // Assert
        Assert.NotNull(workspaces);
        Assert.Empty(workspaces);
    }

    [Fact]
    public async Task ListWorkspacesAsync_MultipleWorkspaces_ReturnsOrderedByLastAccessed()
    {
        // Arrange
        var service = CreateService();
        
        var workspace1 = await service.CreateWorkspaceAsync(new CreateWorkspaceRequest { Name = "workspace-1" });
        await Task.Delay(100); // Ensure different timestamps
        var workspace2 = await service.CreateWorkspaceAsync(new CreateWorkspaceRequest { Name = "workspace-2" });

        // Act
        var workspaces = (await service.ListWorkspacesAsync()).ToList();

        // Assert
        Assert.Equal(2, workspaces.Count);
        // Most recently accessed should be first
        Assert.Equal(workspace2.Id, workspaces[0].Id);
        Assert.Equal(workspace1.Id, workspaces[1].Id);
    }

    [Fact]
    public async Task CleanupWorkspaceAsync_ExistingWorkspace_DeletesWorkspace()
    {
        // Arrange
        var service = CreateService();
        var workspace = await service.CreateWorkspaceAsync(new CreateWorkspaceRequest { Name = "test-workspace" });

        // Act
        var result = await service.CleanupWorkspaceAsync(workspace.Id);

        // Assert
        Assert.True(result);
        Assert.False(Directory.Exists(workspace.Path));
    }

    [Fact]
    public async Task CleanupWorkspaceAsync_NonExistentWorkspace_ReturnsFalse()
    {
        // Arrange
        var service = CreateService();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await service.CleanupWorkspaceAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetStatisticsAsync_NoWorkspaces_ReturnsZeroStatistics()
    {
        // Arrange
        var service = CreateService();

        // Act
        var stats = await service.GetStatisticsAsync();

        // Assert
        Assert.NotNull(stats);
        Assert.Equal(0, stats.TotalWorkspaces);
        Assert.Equal(0, stats.TotalDiskSpaceBytes);
        Assert.Equal(0, stats.WorkspacesWithCachedDependencies);
    }

    [Fact]
    public async Task GetStatisticsAsync_WithWorkspaces_ReturnsCorrectStatistics()
    {
        // Arrange
        var service = CreateService();
        await service.CreateWorkspaceAsync(new CreateWorkspaceRequest { Name = "workspace-1" });
        await service.CreateWorkspaceAsync(new CreateWorkspaceRequest { Name = "workspace-2" });

        // Act
        var stats = await service.GetStatisticsAsync();

        // Assert
        Assert.NotNull(stats);
        Assert.Equal(2, stats.TotalWorkspaces);
        Assert.True(stats.AvailableDiskSpaceBytes > 0);
    }

    [Theory]
    [InlineData(DependencyType.NuGet)]
    [InlineData(DependencyType.Npm)]
    [InlineData(DependencyType.Pip)]
    public void DependencyType_AllTypes_AreDefined(DependencyType dependencyType)
    {
        // Assert - all dependency types should be valid enum values
        Assert.True(Enum.IsDefined(typeof(DependencyType), dependencyType));
    }

    private GitWorkspaceManager CreateService()
    {
        return new GitWorkspaceManager(
            _mockGitService.Object,
            _mockLogger.Object,
            _mockConfiguration.Object);
    }

    public void Dispose()
    {
        // Cleanup test workspace root
        if (Directory.Exists(_testWorkspaceRoot))
        {
            Directory.Delete(_testWorkspaceRoot, recursive: true);
        }
    }
}
