namespace Phase3.AzureDevOps.Tests.Git;

using Microsoft.Extensions.Logging;
using Moq;
using Phase3.AzureDevOps.Exceptions;
using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Models.Git;
using Phase3.AzureDevOps.Services.Git;
using Xunit;

/// <summary>
/// Unit tests for GitService.
/// </summary>
public class GitServiceTests
{
    private readonly Mock<ISecretsProvider> _mockSecretsProvider;
    private readonly Mock<ILogger<GitService>> _mockLogger;

    public GitServiceTests()
    {
        _mockSecretsProvider = new Mock<ISecretsProvider>();
        _mockLogger = new Mock<ILogger<GitService>>();

        // Setup secrets provider
        _mockSecretsProvider
            .Setup(s => s.GetSecretAsync("AzureDevOpsPAT"))
            .ReturnsAsync("test-pat-token");
    }

    [Fact]
    public void CloneRepositoryAsync_EmptyUrl_ThrowsValidationException()
    {
        // Arrange
        var service = CreateService();
        var request = new CloneRepositoryRequest
        {
            RepositoryUrl = "", // Invalid
            LocalPath = "/tmp/test"
        };

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => service.CloneRepositoryAsync(request));
    }

    [Fact]
    public void CloneRepositoryAsync_EmptyLocalPath_ThrowsValidationException()
    {
        // Arrange
        var service = CreateService();
        var request = new CloneRepositoryRequest
        {
            RepositoryUrl = "https://github.com/test/repo.git",
            LocalPath = "" // Invalid
        };

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => service.CloneRepositoryAsync(request));
    }

    [Fact]
    public void CommitChangesAsync_EmptyRepositoryPath_ThrowsValidationException()
    {
        // Arrange
        var service = CreateService();
        var request = new CommitChangesRequest
        {
            RepositoryPath = "", // Invalid
            CommitMessage = "Test commit",
            AuthorName = "Test Author",
            AuthorEmail = "test@example.com"
        };

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => service.CommitChangesAsync(request));
    }

    [Fact]
    public void CommitChangesAsync_EmptyCommitMessage_ThrowsValidationException()
    {
        // Arrange
        var service = CreateService();
        var request = new CommitChangesRequest
        {
            RepositoryPath = "/tmp/repo",
            CommitMessage = "", // Invalid
            AuthorName = "Test Author",
            AuthorEmail = "test@example.com"
        };

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => service.CommitChangesAsync(request));
    }

    [Fact]
    public void CommitChangesAsync_EmptyAuthorName_ThrowsValidationException()
    {
        // Arrange
        var service = CreateService();
        var request = new CommitChangesRequest
        {
            RepositoryPath = "/tmp/repo",
            CommitMessage = "Test commit",
            AuthorName = "", // Invalid
            AuthorEmail = "test@example.com"
        };

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => service.CommitChangesAsync(request));
    }

    [Fact]
    public void CommitChangesAsync_EmptyAuthorEmail_ThrowsValidationException()
    {
        // Arrange
        var service = CreateService();
        var request = new CommitChangesRequest
        {
            RepositoryPath = "/tmp/repo",
            CommitMessage = "Test commit",
            AuthorName = "Test Author",
            AuthorEmail = "" // Invalid
        };

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => service.CommitChangesAsync(request));
    }

    [Fact]
    public void PushChangesAsync_EmptyRepositoryPath_ThrowsValidationException()
    {
        // Arrange
        var service = CreateService();
        var request = new PushChangesRequest
        {
            RepositoryPath = "", // Invalid
            RemoteName = "origin",
            BranchName = "main"
        };

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => service.PushChangesAsync(request));
    }

    [Fact]
    public void PushChangesAsync_EmptyRemoteName_ThrowsValidationException()
    {
        // Arrange
        var service = CreateService();
        var request = new PushChangesRequest
        {
            RepositoryPath = "/tmp/repo",
            RemoteName = "", // Invalid
            BranchName = "main"
        };

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => service.PushChangesAsync(request));
    }

    [Fact]
    public void PullChangesAsync_EmptyRepositoryPath_ThrowsValidationException()
    {
        // Arrange
        var service = CreateService();
        var request = new PullChangesRequest
        {
            RepositoryPath = "", // Invalid
            AuthorName = "Test Author",
            AuthorEmail = "test@example.com"
        };

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => service.PullChangesAsync(request));
    }

    [Fact]
    public void GetCurrentBranch_NonExistentRepository_ThrowsGitOperationException()
    {
        // Arrange
        var service = CreateService();
        var nonExistentPath = "/tmp/nonexistent-repo-" + Guid.NewGuid();

        // Act & Assert
        Assert.Throws<GitOperationException>(() => service.GetCurrentBranch(nonExistentPath));
    }

    [Fact]
    public void HasUncommittedChanges_NonExistentRepository_ThrowsGitOperationException()
    {
        // Arrange
        var service = CreateService();
        var nonExistentPath = "/tmp/nonexistent-repo-" + Guid.NewGuid();

        // Act & Assert
        Assert.Throws<GitOperationException>(() => service.HasUncommittedChanges(nonExistentPath));
    }

    private GitService CreateService()
    {
        return new GitService(
            _mockSecretsProvider.Object,
            _mockLogger.Object);
    }
}
