namespace Phase3.AzureDevOps.Tests.Sync;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Models.Sync;
using Phase3.AzureDevOps.Services.Sync;
using Xunit;

/// <summary>
/// Unit tests for OfflineSyncService.
/// </summary>
public class OfflineSyncServiceTests
{
    private readonly Mock<IWorkItemService> _mockWorkItemService;
    private readonly Mock<ILogger<OfflineSyncService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly string _testDbPath;

    public OfflineSyncServiceTests()
    {
        _mockWorkItemService = new Mock<IWorkItemService>();
        _mockLogger = new Mock<ILogger<OfflineSyncService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        // Use unique test database path
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_offline_{Guid.NewGuid()}.db");
        _mockConfiguration.Setup(c => c["OfflineSync:CacheDbPath"]).Returns(_testDbPath);
    }

    [Fact]
    public async Task EnableOfflineModeAsync_EnablesOfflineMode()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.EnableOfflineModeAsync();

        // Assert
        Assert.True(result);
        
        var status = await service.GetOfflineStatusAsync();
        Assert.True(status.IsOfflineMode);
    }

    [Fact]
    public async Task GetOfflineStatusAsync_InitialState_ReturnsCorrectStatus()
    {
        // Arrange
        var service = CreateService();

        // Act
        var status = await service.GetOfflineStatusAsync();

        // Assert
        Assert.NotNull(status);
        Assert.False(status.IsOfflineMode);
        Assert.Equal(0, status.PendingOperations);
        Assert.Equal(0, status.PendingConflicts);
        Assert.Null(status.LastSyncTime);
    }

    [Fact]
    public async Task SyncChangesAsync_NoPendingOperations_ReturnsEmptyResult()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.SyncChangesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);
        Assert.Empty(result.Conflicts);
    }

    [Fact]
    public async Task GetConflictsAsync_NoConflicts_ReturnsEmptyList()
    {
        // Arrange
        var service = CreateService();

        // Act
        var conflicts = await service.GetConflictsAsync();

        // Assert
        Assert.NotNull(conflicts);
        Assert.Empty(conflicts);
    }

    [Fact]
    public async Task ResolveConflictAsync_NonExistentConflict_ReturnsFalse()
    {
        // Arrange
        var service = CreateService();
        var nonExistentConflictId = Guid.NewGuid();

        // Act
        var result = await service.ResolveConflictAsync(
            nonExistentConflictId, 
            ConflictResolutionPolicy.Abort);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DisableOfflineModeAsync_DisablesOfflineMode()
    {
        // Arrange
        var service = CreateService();
        await service.EnableOfflineModeAsync();

        // Act
        var syncResult = await service.DisableOfflineModeAsync();

        // Assert
        Assert.NotNull(syncResult);
        
        var status = await service.GetOfflineStatusAsync();
        Assert.False(status.IsOfflineMode);
    }

    [Theory]
    [InlineData(ConflictResolutionPolicy.Abort)]
    [InlineData(ConflictResolutionPolicy.Merge)]
    [InlineData(ConflictResolutionPolicy.ManualReview)]
    [InlineData(ConflictResolutionPolicy.ForceOverwrite)]
    public void ConflictResolutionPolicy_AllPolicies_AreDefined(ConflictResolutionPolicy policy)
    {
        // Assert - all policies should be valid enum values
        Assert.True(Enum.IsDefined(typeof(ConflictResolutionPolicy), policy));
    }

    [Theory]
    [InlineData(ConflictType.ModifyConflict)]
    [InlineData(ConflictType.DeleteModifyConflict)]
    [InlineData(ConflictType.ModifyDeleteConflict)]
    public void ConflictType_AllTypes_AreDefined(ConflictType conflictType)
    {
        // Assert - all conflict types should be valid enum values
        Assert.True(Enum.IsDefined(typeof(ConflictType), conflictType));
    }

    private OfflineSyncService CreateService()
    {
        return new OfflineSyncService(
            _mockWorkItemService.Object,
            _mockLogger.Object,
            _mockConfiguration.Object);
    }

    public void Dispose()
    {
        // Cleanup test database
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }
}
