// Copyright (c) Detester. All rights reserved.

namespace Detester;

using Detester.Abstraction;
using Microsoft.Extensions.AI;

/// <summary>
/// Builder class for creating deterministic AI tests.
/// </summary>
public class DetesterBuilder : IDetesterBuilder
{
    private readonly IChatClient chatClient;
    private readonly List<string> prompts = new ();
    private readonly List<string> expectedResponses = new ();

    /// <summary>
    /// Initializes a new instance of the <see cref="DetesterBuilder"/> class.
    /// </summary>
    /// <param name="chatClient">The chat client to use for AI interactions.</param>
    /// <exception cref="ArgumentNullException">Thrown when chatClient is null.</exception>
    public DetesterBuilder(IChatClient chatClient)
    {
        this.chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
    }

    /// <inheritdoc/>
    public IDetesterBuilder WithPrompt(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt cannot be null or whitespace.", nameof(prompt));
        }

        this.prompts.Add(prompt);
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder WithPrompts(params string[] prompts)
    {
        if (prompts == null || prompts.Length == 0)
        {
            throw new ArgumentException("Prompts cannot be null or empty.", nameof(prompts));
        }

        foreach (var prompt in prompts)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ArgumentException("Individual prompts cannot be null or whitespace.", nameof(prompts));
            }

            this.prompts.Add(prompt);
        }

        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder ShouldContainResponse(string expectedText)
    {
        if (string.IsNullOrWhiteSpace(expectedText))
        {
            throw new ArgumentException("Expected text cannot be null or whitespace.", nameof(expectedText));
        }

        this.expectedResponses.Add(expectedText);
        return this;
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (this.prompts.Count == 0)
        {
            throw new DetesterException("No prompts have been added. Use WithPrompt or WithPrompts before executing.");
        }

        var chatHistory = new List<ChatMessage>();

        foreach (var prompt in this.prompts)
        {
            chatHistory.Add(new ChatMessage(ChatRole.User, prompt));

            var response = await this.chatClient.CompleteAsync(chatHistory, cancellationToken: cancellationToken);

            if (response?.Message == null)
            {
                throw new DetesterException($"Received null response for prompt: {prompt}");
            }

            chatHistory.Add(response.Message);

            // Check if response contains expected text for any of the assertions
            if (this.expectedResponses.Count > 0)
            {
                var responseText = response.Message.Text ?? string.Empty;
                var missingExpectations = this.expectedResponses
                    .Where(expected => !responseText.Contains(expected, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (missingExpectations.Count > 0)
                {
                    var missingText = string.Join(", ", missingExpectations.Select(e => $"'{e}'"));
                    throw new DetesterException(
                        $"Response did not contain expected text(s): {missingText}. " +
                        $"Actual response: {responseText}");
                }
            }
        }
    }
}
