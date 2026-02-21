using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Phase3.AzureDevOps.Services.WorkItems;
using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Configuration;

namespace Phase3.AzureDevOps.Tests.WorkItems;

public class WIQLValidatorTests
{
    private readonly WIQLValidator _validator;

    public WIQLValidatorTests()
    {
        _validator = new WIQLValidator();
    }

    [Fact]
    public void IsValid_ValidQuery_ReturnsTrue()
    {
        // Arrange
        string wiql = "SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.State] = 'New'";

        // Act
        var result = _validator.IsValid(wiql);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_SQLInjectionAttempt_ReturnsFalse()
    {
        // Arrange
        string wiql = "SELECT * FROM WorkItems; DROP TABLE WorkItems;";

        // Act
        var result = _validator.IsValid(wiql);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateOrThrow_InvalidQuery_ThrowsArgumentException()
    {
        // Arrange
        string wiql = "SELECT * FROM WorkItems; DROP TABLE WorkItems;";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _validator.ValidateOrThrow(wiql));
    }

    [Fact]
    public void IsValid_MissingSelectClause_ReturnsFalse()
    {
        // Arrange
        string wiql = "FROM WorkItems WHERE [System.State] = 'New'";

        // Act
        var result = _validator.IsValid(wiql);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_MissingFromClause_ReturnsFalse()
    {
        // Arrange
        string wiql = "SELECT [System.Id] WHERE [System.State] = 'New'";

        // Act
        var result = _validator.IsValid(wiql);

        // Assert
        Assert.False(result);
    }
}

public class WorkItemServiceTests
{
    // TODO: Implement WorkItemService tests
    [Fact]
    public void Placeholder()
    {
        Assert.True(true);
    }
}
