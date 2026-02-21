using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Phase3.AzureDevOps.Services.Authentication;

namespace Phase3.AzureDevOps.Tests.Integration;

/// <summary>
/// Integration tests for authentication workflows.
/// These tests verify end-to-end authentication scenarios.
/// </summary>
[Collection("Integration")]
public class AuthenticationIntegrationTests
{
    [Fact]
    public async Task PATAuthentication_EndToEndFlow_Succeeds()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PATAuthenticationProvider>>();
        string validPAT = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOP";
        var provider = new PATAuthenticationProvider(validPAT, loggerMock.Object);

        // Act
        var token = await provider.GetTokenAsync();

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Equal("PAT", provider.AuthenticationMethod);
        Assert.True(provider.IsCached);
    }

    [Fact]
    public void PATAuthentication_InvalidFormat_FailsGracefully()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PATAuthenticationProvider>>();
        string invalidPAT = "short";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new PATAuthenticationProvider(invalidPAT, loggerMock.Object));
        
        Assert.Contains("PAT format is invalid", exception.Message);
    }

    [Fact]
    public async Task MultipleAuthProviders_CanCoexist()
    {
        // Arrange
        var patLoggerMock = new Mock<ILogger<PATAuthenticationProvider>>();
        string validPAT = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOP";
        var patProvider = new PATAuthenticationProvider(validPAT, patLoggerMock.Object);

        // Act
        var patToken = await patProvider.GetTokenAsync();

        // Assert
        Assert.NotNull(patToken);
        Assert.Equal("PAT", patProvider.AuthenticationMethod);
    }
}
