using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Phase3.AzureDevOps.Services.Authentication;

namespace Phase3.AzureDevOps.Tests.Authentication;

public class PATAuthenticationProviderTests
{
    private readonly Mock<ILogger<PATAuthenticationProvider>> _loggerMock;
    private const string ValidPAT = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOP";

    public PATAuthenticationProviderTests()
    {
        _loggerMock = new Mock<ILogger<PATAuthenticationProvider>>();
    }

    [Fact]
    public async Task GetTokenAsync_ValidPAT_ReturnsToken()
    {
        // Arrange
        var provider = new PATAuthenticationProvider(ValidPAT, _loggerMock.Object);

        // Act
        var result = await provider.GetTokenAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Constructor_EmptyPAT_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new PATAuthenticationProvider(string.Empty, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_NullPAT_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new PATAuthenticationProvider(null!, _loggerMock.Object));
    }

    [Fact]
    public void AuthenticationMethod_ReturnsCorrectValue()
    {
        // Arrange
        var provider = new PATAuthenticationProvider(ValidPAT, _loggerMock.Object);

        // Act
        var method = provider.AuthenticationMethod;

        // Assert
        Assert.Equal("PAT", method);
    }

    [Fact]
    public void IsCached_ReturnsTrue()
    {
        // Arrange
        var provider = new PATAuthenticationProvider(ValidPAT, _loggerMock.Object);

        // Act
        var isCached = provider.IsCached;

        // Assert
        Assert.True(isCached);
    }
}
