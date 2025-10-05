// Copyright (c) Detester. All rights reserved.

namespace Detester.Tests;

using Microsoft.Extensions.AI;

/// <summary>
/// Mock implementation of IChatClient for testing.
/// </summary>
public class MockChatClient : IChatClient
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
