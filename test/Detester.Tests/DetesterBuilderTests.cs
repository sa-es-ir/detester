// Copyright (c) Detester. All rights reserved.

namespace Detester.Tests;

using Detester.Abstraction;
using Microsoft.Extensions.AI;

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
        await Assert.ThrowsAsync<DetesterException>(() => builder.AssertAsync());
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
        await builder.WithPrompt("Test prompt").AssertAsync();
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
            .AssertAsync();
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
                .AssertAsync());
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
            .AssertAsync();
    }

    /// <summary>
    /// Mock implementation of IChatClient for testing.
    /// </summary>
    private class MockChatClient : IChatClient
    {
        public string ResponseText { get; set; } = "Mock response";

        public ChatClientMetadata Metadata => new ChatClientMetadata("MockClient");

        public Task<ChatCompletion> CompleteAsync(
            IList<ChatMessage> chatMessages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var response = new ChatCompletion(
                new ChatMessage(ChatRole.Assistant, this.ResponseText));
            return Task.FromResult(response);
        }

        public IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(
            IList<ChatMessage> chatMessages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public TService? GetService<TService>(object? key = null)
            where TService : class
        {
            return null;
        }

        public object? GetService(Type serviceType, object? key = null)
        {
            return null;
        }

        public void Dispose()
        {
        }
    }
}
