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

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var contents = new List<AIContent>();

        if (!string.IsNullOrEmpty(ResponseText))
        {
            contents.Add(new TextContent(ResponseText));
        }

        contents.AddRange(FunctionCallsToReturn);

        var assistantMessage = contents.Count > 0
            ? new ChatMessage(ChatRole.Assistant, contents)
            : new ChatMessage(ChatRole.Assistant, ResponseText);

        // Return a ChatResponse containing the single assistant message.
        var response = new ChatResponse([assistantMessage]);
        return Task.FromResult(response);
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceType == typeof(ChatClientMetadata))
        {
            return Metadata;
        }

        return null;
    }

    public void Dispose()
    {
    }
}
