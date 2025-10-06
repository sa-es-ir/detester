namespace Detester.Tests;

using Detester.Abstraction;

/// <summary>
/// Tests for the DetesterBuilder class.
/// </summary>
public class DetesterBuilderTests
{
    [Fact]
    public void Constructor_WithNullChatClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DetesterBuilder(null!));
    }

    [Fact]
    public void WithInstruction_WithNullInstruction_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithInstruction(null!));
    }

    [Fact]
    public void WithInstruction_WithEmptyInstruction_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithInstruction(string.Empty));
    }

    [Fact]
    public void WithInstruction_WithWhitespaceInstruction_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithInstruction("   "));
    }

    [Fact]
    public void WithInstruction_WithValidInstruction_ReturnsBuilder()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act
        var result = builder.WithInstruction("You are a helpful assistant.");

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IDetesterBuilder>(result);
    }

    [Fact]
    public async Task AssertAsync_WithInstruction_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "This is a helpful response"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithInstruction("You are a helpful assistant.")
            .WithPrompt("Test prompt")
            .ShouldContainResponse("helpful")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithInstructionAndMultiplePrompts_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Response following the instruction"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithInstruction("You are a helpful assistant.")
            .WithPrompt("First prompt")
            .WithPrompt("Second prompt")
            .ShouldContainResponse("Response")
            .ShouldContainResponse("instruction")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithInstructionBeforePrompts_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Instruction-based response"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithInstruction("Answer concisely.")
            .WithPrompt("What is AI?")
            .ShouldContainResponse("Instruction")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithoutInstruction_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Normal response"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPrompt("Test prompt")
            .ShouldContainResponse("Normal")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public void WithPrompt_WithNullPrompt_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithPrompt(null!));
    }

    [Fact]
    public void WithPrompt_WithEmptyPrompt_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithPrompt(string.Empty));
    }

    [Fact]
    public void WithPrompt_WithValidPrompt_ReturnsBuilder()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act
        var result = builder.WithPrompt("Test prompt");

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IDetesterBuilder>(result);
    }

    [Fact]
    public void WithPrompts_WithNullPrompts_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithPrompts(null!));
    }

    [Fact]
    public void WithPrompts_WithEmptyArray_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithPrompts(Array.Empty<string>()));
    }

    [Fact]
    public void WithPrompts_WithValidPrompts_ReturnsBuilder()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act
        var result = builder.WithPrompts("Prompt 1", "Prompt 2");

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IDetesterBuilder>(result);
    }

    [Fact]
    public void ShouldContainResponse_WithNullText_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.ShouldContainResponse(null!));
    }

    [Fact]
    public void ShouldContainResponse_WithEmptyText_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.ShouldContainResponse(string.Empty));
    }

    [Fact]
    public void ShouldContainResponse_WithValidText_ReturnsBuilder()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act
        var result = builder.ShouldContainResponse("Expected text");

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IDetesterBuilder>(result);
    }

    [Fact]
    public async Task AssertAsync_WithoutPrompts_ThrowsDetesterException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await Assert.ThrowsAsync<DetesterException>(() => builder.AssertAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AssertAsync_WithPrompt_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "This is a test response"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder.WithPrompt("Test prompt").AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithMatchingExpectation_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "This is a test response"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPrompt("Test prompt")
            .ShouldContainResponse("test response")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithNonMatchingExpectation_ThrowsDetesterException()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "This is a test response"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await Assert.ThrowsAsync<DetesterException>(() =>
            builder
                .WithPrompt("Test prompt")
                .ShouldContainResponse("missing text")
                .AssertAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AssertAsync_SupportsMethodChaining()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Response with expected keywords"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPrompt("First prompt")
            .WithPrompt("Second prompt")
            .ShouldContainResponse("expected")
            .ShouldContainResponse("keywords")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public void OrShouldContainResponse_WithNullText_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            builder.ShouldContainResponse("test").OrShouldContainResponse(null!));
    }

    [Fact]
    public void OrShouldContainResponse_WithEmptyText_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            builder.ShouldContainResponse("test").OrShouldContainResponse(string.Empty));
    }

    [Fact]
    public void OrShouldContainResponse_WithValidText_ReturnsBuilder()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act
        var result = builder
            .ShouldContainResponse("test")
            .OrShouldContainResponse("alternative");

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IDetesterBuilder>(result);
    }

    [Fact]
    public void OrShouldContainResponse_WithoutPriorAssertion_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            builder.OrShouldContainResponse("test"));
    }

    [Fact]
    public async Task AssertAsync_WithOrShouldContainResponse_FirstOptionMatches_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Response with expected keywords"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPrompt("Test prompt")
            .ShouldContainResponse("expected")
            .OrShouldContainResponse("alternative")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithOrShouldContainResponse_SecondOptionMatches_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Response with alternative keywords"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPrompt("Test prompt")
            .ShouldContainResponse("expected")
            .OrShouldContainResponse("alternative")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithOrShouldContainResponse_NoOptionMatches_ThrowsDetesterException()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Response with different keywords"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DetesterException>(() =>
            builder
                .WithPrompt("Test prompt")
                .ShouldContainResponse("expected")
                .OrShouldContainResponse("alternative")
                .AssertAsync(TestContext.Current.CancellationToken));

        Assert.Contains("expected' OR 'alternative", exception.Message);
    }

    [Fact]
    public async Task AssertAsync_WithMultipleOrOptions_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Response with keywords 2"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPrompt("First prompt")
            .WithPrompt("Second prompt")
            .ShouldContainResponse("expected")
            .OrShouldContainResponse("keywords 1")
            .OrShouldContainResponse("keywords 2")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithMixedAndOrAssertions_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Response with expected and alternative keywords"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPrompt("Test prompt")
            .ShouldContainResponse("Response")
            .ShouldContainResponse("expected")
            .OrShouldContainResponse("missing")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithMixedAndOrAssertions_MissingAndAssertion_ThrowsDetesterException()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Response with alternative keywords"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await Assert.ThrowsAsync<DetesterException>(() =>
            builder
                .WithPrompt("Test prompt")
                .ShouldContainResponse("missing")
                .ShouldContainResponse("expected")
                .OrShouldContainResponse("alternative")
                .AssertAsync(TestContext.Current.CancellationToken));
    }
}
