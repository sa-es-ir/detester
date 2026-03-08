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

    /// <summary>
    /// Gets or sets optional usage details to include in the response (for token assertion tests).
    /// </summary>
    public UsageDetails? UsageDetailsToReturn { get; set; }

    /// <summary>
    /// Gets or sets an optional delay in milliseconds before returning a response (for latency assertion tests).
    /// </summary>
    public int ResponseDelayMs { get; set; }

    public ChatClientMetadata Metadata => new ChatClientMetadata("MockClient");

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (ResponseDelayMs > 0)
        {
            await Task.Delay(ResponseDelayMs, cancellationToken);
        }

        var contents = new List<AIContent>();

        if (!string.IsNullOrEmpty(ResponseText))
        {
            contents.Add(new TextContent(ResponseText));
        }

        contents.AddRange(FunctionCallsToReturn);

        var assistantMessage = contents.Count > 0
            ? new ChatMessage(ChatRole.Assistant, contents)
            : new ChatMessage(ChatRole.Assistant, ResponseText);

        var response = new ChatResponse([assistantMessage])
        {
            Usage = UsageDetailsToReturn,
        };

        return response;
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
