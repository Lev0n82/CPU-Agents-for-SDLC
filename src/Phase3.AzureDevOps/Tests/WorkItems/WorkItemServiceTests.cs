using Xunit;
using Phase3.AzureDevOps.Services.WorkItems;

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

