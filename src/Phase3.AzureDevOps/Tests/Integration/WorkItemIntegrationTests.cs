using Xunit;
using Phase3.AzureDevOps.Services.WorkItems;

namespace Phase3.AzureDevOps.Tests.Integration;

/// <summary>
/// Integration tests for work item operations.
/// These tests verify end-to-end work item workflows including WIQL validation.
/// </summary>
[Collection("Integration")]
public class WorkItemIntegrationTests
{
    [Fact]
    public void WIQLValidation_ComplexScenarios_WorksCorrectly()
    {
        // Arrange
        var validator = new WIQLValidator();
        
        var validQueries = new[]
        {
            "SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.State] = 'New'",
            "SELECT [System.Id], [System.Title], [System.State] FROM WorkItems ORDER BY [System.ChangedDate] DESC"
        };
        
        var invalidQueries = new[]
        {
            "SELECT * FROM WorkItems; DROP TABLE WorkItems;",
            "SELECT * FROM WorkItems WHERE 1=1; DELETE FROM WorkItems;"
        };

        // Act & Assert - Valid queries
        foreach (var query in validQueries)
        {
            Assert.True(validator.IsValid(query), $"Query should be valid: {query}");
        }

        // Act & Assert - Invalid queries
        foreach (var query in invalidQueries)
        {
            Assert.False(validator.IsValid(query), $"Query should be invalid: {query}");
        }
    }

    [Fact]
    public void WIQLValidation_EdgeCases_HandledCorrectly()
    {
        // Arrange
        var validator = new WIQLValidator();

        // Act & Assert - Empty query
        Assert.False(validator.IsValid(string.Empty));
        Assert.False(validator.IsValid(null!));
        Assert.False(validator.IsValid("   "));

        // Act & Assert - Missing clauses
        Assert.False(validator.IsValid("FROM WorkItems"));
        Assert.False(validator.IsValid("SELECT [System.Id]"));
        Assert.False(validator.IsValid("WHERE [System.State] = 'New'"));
    }

    [Fact]
    public void WIQLValidation_SQLInjectionAttempts_AllBlocked()
    {
        // Arrange
        var validator = new WIQLValidator();
        
        var injectionAttempts = new[]
        {
            "SELECT * FROM WorkItems; DROP TABLE Users;",
            "SELECT [System.Id] FROM WorkItems WHERE [System.Title] = 'Test'; DELETE FROM WorkItems;",
            "SELECT [System.Id] FROM WorkItems; EXEC sp_executesql 'DROP TABLE WorkItems'"
        };

        // Act & Assert
        foreach (var attempt in injectionAttempts)
        {
            Assert.False(validator.IsValid(attempt), $"Injection attempt should be blocked: {attempt}");
            Assert.Throws<ArgumentException>(() => validator.ValidateOrThrow(attempt));
        }
    }
}
