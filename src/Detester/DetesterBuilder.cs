namespace Detester;

using Detester.Abstraction;
using Microsoft.Extensions.AI;

/// <summary>
/// Builder class for creating deterministic AI tests.
/// </summary>
public class DetesterBuilder : IDetesterBuilder
{
    private readonly IChatClient chatClient;
    private readonly List<string> prompts = [];
    private readonly List<string> expectedResponses = [];
    private readonly List<List<string>> orResponseGroups = [];
    private string? instruction;

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
    public IDetesterBuilder WithInstruction(string instruction)
    {
        if (string.IsNullOrWhiteSpace(instruction))
        {
            throw new ArgumentException("Instruction cannot be null or whitespace.", nameof(instruction));
        }

        this.instruction = instruction;
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder WithPrompt(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt cannot be null or whitespace.", nameof(prompt));
        }

        prompts.Add(prompt);
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

        expectedResponses.Add(expectedText);
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder OrShouldContainResponse(string expectedText)
    {
        if (string.IsNullOrWhiteSpace(expectedText))
        {
            throw new ArgumentException("Expected text cannot be null or whitespace.", nameof(expectedText));
        }

        // If there are no existing expectations, treat this as a new OR group
        if (expectedResponses.Count == 0 && orResponseGroups.Count == 0)
        {
            throw new InvalidOperationException("OrShouldContainResponse must be called after ShouldContainResponse or another OrShouldContainResponse.");
        }

        // If the last expectation was an AND (in expectedResponses), move it to a new OR group
        if (expectedResponses.Count > 0)
        {
            var lastExpectation = expectedResponses[expectedResponses.Count - 1];
            expectedResponses.RemoveAt(expectedResponses.Count - 1);
            orResponseGroups.Add(new List<string> { lastExpectation, expectedText });
        }
        else
        {
            // Add to the last OR group
            orResponseGroups[orResponseGroups.Count - 1].Add(expectedText);
        }

        return this;
    }

    /// <inheritdoc/>
    public async Task AssertAsync(CancellationToken cancellationToken = default)
    {
        if (prompts.Count == 0)
        {
            throw new DetesterException("No prompts have been added. Use WithPrompt or WithPrompts before asserting.");
        }

        var chatHistory = new List<ChatMessage>();

        // Add instruction as system message if provided
        if (!string.IsNullOrWhiteSpace(instruction))
        {
            chatHistory.Add(new ChatMessage(ChatRole.System, instruction));
        }

        foreach (var prompt in prompts)
        {
            chatHistory.Add(new ChatMessage(ChatRole.User, prompt));

            var response = await chatClient.CompleteAsync(chatHistory, cancellationToken: cancellationToken);

            if (response?.Message == null)
            {
                throw new DetesterException($"Received null response for prompt: {prompt}");
            }

            chatHistory.Add(response.Message);

            // Check if response contains expected text for any of the assertions
            if (expectedResponses.Count > 0 || orResponseGroups.Count > 0)
            {
                var responseText = response.Message.Text ?? string.Empty;

                // Check AND assertions
                var missingExpectations = expectedResponses
                    .Where(expected => !responseText.Contains(expected, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (missingExpectations.Count > 0)
                {
                    var missingText = string.Join(", ", missingExpectations.Select(e => $"'{e}'"));
                    throw new DetesterException(
                        $"Response did not contain expected text(s): {missingText}. " +
                        $"Actual response: {responseText}");
                }

                // Check OR assertions (at least one in each OR group must match)
                foreach (var orGroup in orResponseGroups)
                {
                    var hasMatch = orGroup.Any(expected =>
                        responseText.Contains(expected, StringComparison.OrdinalIgnoreCase));

                    if (!hasMatch)
                    {
                        var orOptions = string.Join("' OR '", orGroup);
                        throw new DetesterException(
                            $"Response did not contain any of the expected alternatives: '{orOptions}'. " +
                            $"Actual response: {responseText}");
                    }
                }
            }
        }
    }
}
