// Copyright (c) Detester. All rights reserved.

namespace Detester.Tests;

using Microsoft.Extensions.AI;

/// <summary>
/// A mock <see cref="IChatClient"/> that delegates response generation to a callback function.
/// Useful for testing scenarios where the response needs to vary per call (e.g., alternating pass/fail).
/// </summary>
public class CallbackMockChatClient : IChatClient
{
    private readonly Func<IEnumerable<ChatMessage>, string> responseCallback;

    /// <summary>
    /// Initializes a new instance of the <see cref="CallbackMockChatClient"/> class.
    /// </summary>
    /// <param name="responseCallback">A function that receives the chat messages and returns the response text.</param>
    public CallbackMockChatClient(Func<IEnumerable<ChatMessage>, string> responseCallback)
    {
        this.responseCallback = responseCallback ?? throw new ArgumentNullException(nameof(responseCallback));
    }

    public ChatClientMetadata Metadata => new ChatClientMetadata("CallbackMockClient");

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var text = responseCallback(messages);
        var message = new ChatMessage(ChatRole.Assistant, text);
        return Task.FromResult(new ChatResponse([message]));
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
