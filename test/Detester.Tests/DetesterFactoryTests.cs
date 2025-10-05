namespace Detester.Tests;

using Detester.Abstraction;
using Microsoft.Extensions.AI;

/// <summary>
/// Tests for the DetesterFactory class.
/// </summary>
public partial class DetesterFactoryTests
{
    [Fact]
    public void CreateWithOpenAI_WithNullApiKey_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            DetesterFactory.CreateWithOpenAI(null!, "gpt-4"));
    }

    [Fact]
    public void CreateWithOpenAI_WithEmptyApiKey_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            DetesterFactory.CreateWithOpenAI(string.Empty, "gpt-4"));
    }

    [Fact]
    public void CreateWithOpenAI_WithNullModelName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            DetesterFactory.CreateWithOpenAI("test-key", null!));
    }

    [Fact]
    public void CreateWithAzureOpenAI_WithNullEndpoint_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            DetesterFactory.CreateWithAzureOpenAI(null!, "key", "deployment"));
    }

    [Fact]
    public void CreateWithAzureOpenAI_WithNullApiKey_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            DetesterFactory.CreateWithAzureOpenAI("https://test.com", null!, "deployment"));
    }

    [Fact]
    public void CreateWithAzureOpenAI_WithNullDeploymentName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            DetesterFactory.CreateWithAzureOpenAI("https://test.com", "key", null!));
    }

    [Fact]
    public void Create_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            DetesterFactory.Create((DetesterOptions)null!));
    }

    [Fact]
    public void Create_WithUnconfiguredOptions_ThrowsDetesterException()
    {
        // Arrange
        var options = new DetesterOptions();

        // Act & Assert
        var exception = Assert.Throws<DetesterException>(() =>
            DetesterFactory.Create(options));

        Assert.Contains("Neither OpenAI nor Azure OpenAI", exception.Message);
    }

    [Fact]
    public void Create_WithOpenAIConfiguration_ReturnsBuilder()
    {
        // Arrange
        var options = new DetesterOptions
        {
            OpenAIApiKey = "test-key",
            ModelName = "gpt-4"
        };

        // Act
        var builder = DetesterFactory.Create(options);

        // Assert
        Assert.NotNull(builder);
        Assert.IsAssignableFrom<IDetesterBuilder>(builder);
    }

    [Fact]
    public void Create_WithAzureOpenAIConfiguration_ReturnsBuilder()
    {
        // Arrange
        var options = new DetesterOptions
        {
            AzureOpenAIEndpoint = "https://test.openai.azure.com",
            AzureOpenAIApiKey = "test-key",
            ModelName = "gpt-4-deployment"
        };

        // Act
        var builder = DetesterFactory.Create(options);

        // Assert
        Assert.NotNull(builder);
        Assert.IsAssignableFrom<IDetesterBuilder>(builder);
    }

    [Fact]
    public void Create_WithAzureOpenAIWithoutModelName_ThrowsDetesterException()
    {
        // Arrange
        var options = new DetesterOptions
        {
            AzureOpenAIEndpoint = "https://test.openai.azure.com",
            AzureOpenAIApiKey = "test-key"
        };

        // Act & Assert
        var exception = Assert.Throws<DetesterException>(() =>
            DetesterFactory.Create(options));

        Assert.Contains("ModelName is required", exception.Message);
    }

    [Fact]
    public void Create_WithNullChatClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            DetesterFactory.Create((IChatClient)null!));
    }

    [Fact]
    public void Create_WithCustomChatClient_ReturnsBuilder()
    {
        // Arrange
        var mockClient = new MockChatClient();

        // Act
        var builder = DetesterFactory.Create(mockClient);

        // Assert
        Assert.NotNull(builder);
        Assert.IsAssignableFrom<IDetesterBuilder>(builder);
    }
}
