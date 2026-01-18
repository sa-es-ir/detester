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
    private readonly List<string> unexpectedResponses = [];
    private readonly List<string> unexpectedAnyResponses = [];
    private readonly List<string> regexPatterns = [];
    private readonly List<string> containAllSubstrings = [];
    private readonly List<List<string>> containAnyGroups = [];
    private readonly List<EqualityExpectation> equalityExpectations = [];
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
    public IDetesterBuilder ShouldNotContainResponse(string unexpectedText)
    {
        if (string.IsNullOrWhiteSpace(unexpectedText))
        {
            throw new ArgumentException("Unexpected text cannot be null or whitespace.", nameof(unexpectedText));
        }

        unexpectedResponses.Add(unexpectedText);
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder ShouldNotContainAnyResponse(params string[] unexpectedTexts)
    {
        if (unexpectedTexts == null || unexpectedTexts.Length == 0)
        {
            throw new ArgumentException("Unexpected texts cannot be null or empty.", nameof(unexpectedTexts));
        }

        foreach (var text in unexpectedTexts)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Individual unexpected texts cannot be null or whitespace.", nameof(unexpectedTexts));
            }

            unexpectedAnyResponses.Add(text);
        }

        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder ShouldMatchRegex(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("Pattern cannot be null or whitespace.", nameof(pattern));
        }

        regexPatterns.Add(pattern);
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder ShouldNotContain(string unexpectedText)
    {
        return ShouldNotContainResponse(unexpectedText);
    }

    /// <inheritdoc/>
    public IDetesterBuilder ShouldContainAll(params string[] expectedSubstrings)
    {
        if (expectedSubstrings == null || expectedSubstrings.Length == 0)
        {
            throw new ArgumentException("Expected substrings cannot be null or empty.", nameof(expectedSubstrings));
        }

        foreach (var substring in expectedSubstrings)
        {
            if (string.IsNullOrWhiteSpace(substring))
            {
                throw new ArgumentException("Individual expected substrings cannot be null or whitespace.", nameof(expectedSubstrings));
            }

            containAllSubstrings.Add(substring);
        }

        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder ShouldContainAny(params string[] expectedSubstrings)
    {
        if (expectedSubstrings == null || expectedSubstrings.Length == 0)
        {
            throw new ArgumentException("Expected substrings cannot be null or empty.", nameof(expectedSubstrings));
        }

        var group = new List<string>();

        foreach (var substring in expectedSubstrings)
        {
            if (string.IsNullOrWhiteSpace(substring))
            {
                throw new ArgumentException("Individual expected substrings cannot be null or whitespace.", nameof(expectedSubstrings));
            }

            group.Add(substring);
        }

        containAnyGroups.Add(group);
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder ShouldBeEqualTo(string expected, StringComparison comparison = StringComparison.Ordinal)
    {
        if (expected is null)
        {
            throw new ArgumentNullException(nameof(expected));
        }

        equalityExpectations.Add(new EqualityExpectation
        {
            Expected = expected,
            Comparison = comparison,
        });

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

            var response = await chatClient.GetResponseAsync(chatHistory, cancellationToken: cancellationToken);

            if (response?.Text == null)
            {
                throw new DetesterException($"Received null response for prompt: {prompt}");
            }

            chatHistory.Add(new ChatMessage(ChatRole.Assistant, response.Text));

            // Check if response contains expected or unexpected text for any of the assertions
            if (expectedResponses.Count > 0 || orResponseGroups.Count > 0 ||
                unexpectedResponses.Count > 0 || unexpectedAnyResponses.Count > 0 ||
                regexPatterns.Count > 0 || containAllSubstrings.Count > 0 ||
                containAnyGroups.Count > 0 || equalityExpectations.Count > 0)
            {
                var responseText = response.Text ?? string.Empty;

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

                // Check NOT-CONTAINS assertions (single)
                var violatingUnexpected = unexpectedResponses
                    .Where(unexpected => responseText.Contains(unexpected, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (violatingUnexpected.Count > 0)
                {
                    var violatingText = string.Join(", ", violatingUnexpected.Select(e => $"'{e}'"));
                    throw new DetesterException(
                        $"Response contained unexpected text(s): {violatingText}. " +
                        $"Actual response: {responseText}");
                }

                // Check NOT-CONTAINS-ANY assertions (any of the registered texts must be absent)
                var violatingAny = unexpectedAnyResponses
                    .Where(unexpected => responseText.Contains(unexpected, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (violatingAny.Count > 0)
                {
                    var violatingText = string.Join(", ", violatingAny.Select(e => $"'{e}'"));
                    throw new DetesterException(
                        $"Response contained one or more texts that should not appear: {violatingText}. " +
                        $"Actual response: {responseText}");
                }

                // Check regex patterns
                foreach (var pattern in regexPatterns)
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(responseText, pattern))
                    {
                        throw new DetesterException(
                            $"Response did not match the required regular expression pattern '{pattern}'. " +
                            $"Actual response: {responseText}");
                    }
                }

                // Check that response contains all required substrings
                var missingAllSubstrings = containAllSubstrings
                    .Where(s => !responseText.Contains(s, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (missingAllSubstrings.Count > 0)
                {
                    var missingText = string.Join(", ", missingAllSubstrings.Select(e => $"'{e}'"));
                    throw new DetesterException(
                        $"Response did not contain all required substrings: {missingText}. " +
                        $"Actual response: {responseText}");
                }

                // Check ANY-groups (at least one in each group must match)
                foreach (var group in containAnyGroups)
                {
                    var hasAny = group.Any(s => responseText.Contains(s, StringComparison.OrdinalIgnoreCase));
                    if (!hasAny)
                    {
                        var options = string.Join("' OR '", group);
                        throw new DetesterException(
                            $"Response did not contain any of the required alternatives: '{options}'. " +
                            $"Actual response: {responseText}");
                    }
                }

                // Check equality expectations
                foreach (var expectation in equalityExpectations)
                {
                    if (!string.Equals(responseText, expectation.Expected, expectation.Comparison))
                    {
                        throw new DetesterException(
                            $"Response was not equal to the expected text. Expected: '{expectation.Expected}', Actual: '{responseText}'.");
                    }
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
                // Extract function calls from assistant messages in the ChatResponse
                var functionCalls = response.Messages
                    .Where(m => m.Role == ChatRole.Assistant)
                    .SelectMany(m => m.Contents.OfType<FunctionCallContent>())
                    .ToList();

                foreach (var expectation in expectedFunctionCalls)
                {
                    FunctionCallContent? matchedCall = null;

                    foreach (var functionCall in functionCalls)
                    {
                        if (functionCall.Name == expectation.FunctionName)
                        {
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
                                matchedCall = functionCall;
                                break;
                            }
                        }
                    }

                    if (matchedCall == null)
                    {
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
                                $"Expected function '{expectation.FunctionName}' to be called with parameters ({expectedParams}), but it was not called with those parameters. Actual function calls: {actualFunctions}");
                        }
                        else
                        {
                            throw new DetesterException(
                                $"Expected function '{expectation.FunctionName}' to be called, but only these functions were called: {actualFunctions}");
                        }
                    }

                    functionCalls.Remove(matchedCall);
                }
            }

            // Check JSON expectations
            if (jsonExpectations.Count > 0)
            {
                var responseText = response.Text ?? string.Empty;

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

    private sealed class FunctionCallExpectation
    {
        public string FunctionName { get; set; } = string.Empty;

        public IDictionary<string, object?>? ExpectedParameters { get; set; }
    }

    private sealed class JsonExpectation
    {
        public Type TargetType { get; set; } = typeof(object);

        public JsonSerializerOptions? Options { get; set; }

        public Delegate? Validator { get; set; }
    }

    private sealed class EqualityExpectation
    {
        public string Expected { get; set; } = string.Empty;

        public StringComparison Comparison { get; set; }
    }
}
