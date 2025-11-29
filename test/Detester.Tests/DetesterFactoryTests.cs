namespace Detester.Tests;

using Detester.Abstraction;
using Microsoft.Extensions.AI;

/// <summary>
/// Tests for the DetesterFactory class.
/// </summary>
public partial class DetesterFactoryTests
{
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
