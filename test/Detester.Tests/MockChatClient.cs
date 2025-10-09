// Copyright (c) Detester. All rights reserved.

namespace Detester.Tests;

using Microsoft.Extensions.AI;

/// <summary>
/// Mock implementation of IChatClient for testing.
/// </summary>
public class MockChatClient : IChatClient
{
    public string ResponseText { get; set; } = "Mock response";

    public List<FunctionCallContent> FunctionCallsToReturn { get; set; } = [];

    public ChatClientMetadata Metadata => new ChatClientMetadata("MockClient");

    public Task<ChatCompletion> CompleteAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var contents = new List<AIContent>();

        // Add text content if ResponseText is set
        if (!string.IsNullOrEmpty(ResponseText))
        {
            contents.Add(new TextContent(ResponseText));
        }

        // Add function calls
        contents.AddRange(FunctionCallsToReturn);

        var message = contents.Count > 0
            ? new ChatMessage(ChatRole.Assistant, contents)
            : new ChatMessage(ChatRole.Assistant, ResponseText);

        var response = new ChatCompletion(message);
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
