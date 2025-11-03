namespace Detester;

using Detester.Abstraction;
using Microsoft.Extensions.AI;
using System.Text.Json;

/// <summary>
/// Builder class for creating deterministic AI tests.
/// </summary>
public class DetesterBuilder : IDetesterBuilder
{
    private readonly IChatClient chatClient;
    private readonly List<string> prompts = [];
    private readonly List<string> expectedResponses = [];
    private readonly List<List<string>> orResponseGroups = [];
    private readonly List<FunctionCallExpectation> expectedFunctionCalls = [];
    private readonly List<JsonExpectation> jsonExpectations = [];
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
    public IDetesterBuilder WithInstructionFromFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));
        }

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension != ".md" && extension != ".txt")
        {
            throw new ArgumentException("File must be a markdown (.md) or text (.txt) file.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}", filePath);
        }

        var content = File.ReadAllText(filePath);

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("File content cannot be empty or whitespace.", nameof(filePath));
        }

        this.instruction = content;
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
    public IDetesterBuilder WithPromptFromFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));
        }

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension != ".md" && extension != ".txt")
        {
            throw new ArgumentException("File must be a markdown (.md) or text (.txt) file.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}", filePath);
        }

        var content = File.ReadAllText(filePath);

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("File content cannot be empty or whitespace.", nameof(filePath));
        }

        prompts.Add(content);
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
    public IDetesterBuilder ShouldCallFunction(string functionName)
    {
        if (string.IsNullOrWhiteSpace(functionName))
        {
            throw new ArgumentException("Function name cannot be null or whitespace.", nameof(functionName));
        }

        expectedFunctionCalls.Add(new FunctionCallExpectation { FunctionName = functionName });
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder ShouldCallFunctionWithParameters(string functionName, IDictionary<string, object?> expectedParameters)
    {
        if (string.IsNullOrWhiteSpace(functionName))
        {
            throw new ArgumentException("Function name cannot be null or whitespace.", nameof(functionName));
        }

        if (expectedParameters == null)
        {
            throw new ArgumentNullException(nameof(expectedParameters));
        }

        expectedFunctionCalls.Add(new FunctionCallExpectation
        {
            FunctionName = functionName,
            ExpectedParameters = expectedParameters
        });
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder ShouldHaveJsonOfType<T>(JsonSerializerOptions? options = null, Func<T, bool>? validator = null)
    {
        jsonExpectations.Add(new JsonExpectation
        {
            TargetType = typeof(T),
            Options = options,
            Validator = validator
        });
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

            // Check function call expectations
            if (expectedFunctionCalls.Count > 0)
            {
                var functionCalls = response.Message.Contents
                    .OfType<FunctionCallContent>()
                    .ToList();

                // Create a working copy of expectations to match
                var remainingExpectations = new List<FunctionCallExpectation>(expectedFunctionCalls);

                foreach (var expectation in expectedFunctionCalls)
                {
                    // Find a matching function call
                    FunctionCallContent? matchedCall = null;

                    foreach (var functionCall in functionCalls)
                    {
                        if (functionCall.Name == expectation.FunctionName)
                        {
                            // Check if parameters match (if specified)
                            if (expectation.ExpectedParameters != null)
                            {
                                if (ParametersMatch(functionCall.Arguments, expectation.ExpectedParameters))
                                {
                                    matchedCall = functionCall;
                                    break;
                                }
                            }
                            else
                            {
                                // No parameter verification needed, just match by name
                                matchedCall = functionCall;
                                break;
                            }
                        }
                    }

                    if (matchedCall == null)
                    {
                        // Build detailed error message
                        if (functionCalls.Count == 0)
                        {
                            throw new DetesterException(
                                $"Expected function '{expectation.FunctionName}' to be called, but no function calls were made.");
                        }

                        var actualFunctions = string.Join(", ", functionCalls.Select(f => $"'{f.Name}'"));

                        if (expectation.ExpectedParameters != null)
                        {
                            var expectedParams = string.Join(", ", expectation.ExpectedParameters.Select(p => $"{p.Key}={p.Value}"));
                            throw new DetesterException(
                                $"Expected function '{expectation.FunctionName}' to be called with parameters ({expectedParams}), " +
                                $"but it was not called with those parameters. " +
                                $"Actual function calls: {actualFunctions}");
                        }
                        else
                        {
                            throw new DetesterException(
                                $"Expected function '{expectation.FunctionName}' to be called, " +
                                $"but only these functions were called: {actualFunctions}");
                        }
                    }

                    // Remove the matched call so it won't be matched again
                    functionCalls.Remove(matchedCall);
                }
            }

            // Check JSON expectations
            if (jsonExpectations.Count > 0)
            {
                var responseText = response.Message.Text ?? string.Empty;

                foreach (var expectation in jsonExpectations)
                {
                    try
                    {
                        // Attempt to deserialize the response text as JSON
                        var deserializedObject = JsonSerializer.Deserialize(responseText, expectation.TargetType, expectation.Options);

                        if (deserializedObject == null)
                        {
                            throw new DetesterException(
                                $"Failed to deserialize JSON response to type '{expectation.TargetType.Name}': " +
                                $"Deserialization resulted in null. Response: {responseText}");
                        }

                        // If a validator is provided, invoke it
                        if (expectation.Validator != null)
                        {
                            var result = expectation.Validator.DynamicInvoke(deserializedObject);
                            if (result == null)
                            {
                                throw new DetesterException(
                                    $"JSON response validation returned null for type '{expectation.TargetType.Name}'. " +
                                    $"The validation predicate must return a boolean value. " +
                                    $"Response: {responseText}");
                            }

                            var validationResult = (bool)result;
                            if (!validationResult)
                            {
                                throw new DetesterException(
                                    $"JSON response validation failed for type '{expectation.TargetType.Name}'. " +
                                    $"The deserialized object did not pass the validation predicate. " +
                                    $"Response: {responseText}");
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        throw new DetesterException(
                            $"Failed to deserialize JSON response to type '{expectation.TargetType.Name}': {ex.Message}. " +
                            $"Response: {responseText}", ex);
                    }
                }
            }
        }
    }

    private static bool ParametersMatch(IDictionary<string, object?>? actual, IDictionary<string, object?> expected)
    {
        if (actual == null)
        {
            return expected.Count == 0;
        }

        // Check if all expected parameters are present and have matching values
        foreach (var expectedParam in expected)
        {
            if (!actual.TryGetValue(expectedParam.Key, out var actualValue))
            {
                return false;
            }

            // Compare values
            if (!ValuesEqual(actualValue, expectedParam.Value))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ValuesEqual(object? actual, object? expected)
    {
        if (actual == null && expected == null)
        {
            return true;
        }

        if (actual == null || expected == null)
        {
            return false;
        }

        // Handle string comparison case-insensitively
        if (actual is string actualStr && expected is string expectedStr)
        {
            return actualStr.Equals(expectedStr, StringComparison.OrdinalIgnoreCase);
        }

        // For other types, use standard equality
        return actual.Equals(expected);
    }
}
