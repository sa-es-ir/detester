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
    private readonly ChatOptions? mainChatOptions;
    private readonly List<PromptStep> steps = [];
    private PromptStep? pendingStep;
    private string? instruction;
    private TimeSpan? maxLatency;
    private int? maxTotalTokens;
    private int? maxCompletionTokens;
    private IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator;
    private IChatClient? judge;

    /// <summary>
    /// Initializes a new instance of the <see cref="DetesterBuilder"/> class.
    /// </summary>
    /// <param name="chatClient">The chat client to use for AI interactions.</param>
    /// <exception cref="ArgumentNullException">Thrown when chatClient is null.</exception>
    public DetesterBuilder(IChatClient chatClient)
    {
        this.chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DetesterBuilder"/> class.
    /// </summary>
    /// <param name="chatClient">The chat client to be used for communication. Cannot be null.</param>
    /// <param name="chatOptions">The configuration options for the chat client. Specifies behavior, settings and tools for chat operations.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="chatClient"/> is null.</exception>
    public DetesterBuilder(IChatClient chatClient, ChatOptions chatOptions)
    {
        this.chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
        this.mainChatOptions = chatOptions;
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
        this.instruction = ReadTextFile(filePath);
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder WithPrompt(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt cannot be null or whitespace.", nameof(prompt));
        }

        AddPromptStep(prompt);
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder WithPromptFromFile(string filePath)
    {
        AddPromptStep(ReadTextFile(filePath));
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

            AddPromptStep(prompt);
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

        GetCurrentStep().ExpectedResponses.Add(expectedText);
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder ShouldNotContainResponse(string unexpectedText)
    {
        if (string.IsNullOrWhiteSpace(unexpectedText))
        {
            throw new ArgumentException("Unexpected text cannot be null or whitespace.", nameof(unexpectedText));
        }

        GetCurrentStep().UnexpectedResponses.Add(unexpectedText);
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

            GetCurrentStep().UnexpectedAnyResponses.Add(text);
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

        GetCurrentStep().RegexPatterns.Add(pattern);
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

            GetCurrentStep().ContainAllSubstrings.Add(substring);
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

        GetCurrentStep().ContainAnyGroups.Add(group);
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder ShouldBeEqualTo(string expected, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        if (expected is null)
        {
            throw new ArgumentNullException(nameof(expected));
        }

        GetCurrentStep().EqualityExpectations.Add(new EqualityExpectation
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

        var step = GetCurrentStep();

        if (step.ExpectedResponses.Count == 0 && step.OrResponseGroups.Count == 0)
        {
            throw new InvalidOperationException("OrShouldContainResponse must be called after ShouldContainResponse or another OrShouldContainResponse.");
        }

        if (step.ExpectedResponses.Count > 0)
        {
            var lastExpectation = step.ExpectedResponses[step.ExpectedResponses.Count - 1];
            step.ExpectedResponses.RemoveAt(step.ExpectedResponses.Count - 1);
            step.OrResponseGroups.Add(new List<string> { lastExpectation, expectedText });
        }
        else
        {
            step.OrResponseGroups[step.OrResponseGroups.Count - 1].Add(expectedText);
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

        GetCurrentStep().ExpectedFunctionCalls.Add(new FunctionCallExpectation { FunctionName = functionName });
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

        GetCurrentStep().ExpectedFunctionCalls.Add(new FunctionCallExpectation
        {
            FunctionName = functionName,
            ExpectedParameters = expectedParameters,
        });
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder ShouldNotCallFunction(string functionName)
    {
        if (string.IsNullOrWhiteSpace(functionName))
        {
            throw new ArgumentException("Function name cannot be null or whitespace.", nameof(functionName));
        }

        GetCurrentStep().NotExpectedFunctionCalls.Add(functionName);
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder ShouldHaveJsonOfType<T>(JsonSerializerOptions? options = null, Func<T, bool>? validator = null)
    {
        GetCurrentStep().JsonExpectations.Add(new JsonExpectation
        {
            TargetType = typeof(T),
            Options = options,
            Validator = validator,
        });
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder WithEmbeddingGenerator(IEmbeddingGenerator<string, Embedding<float>> generator)
    {
        this.embeddingGenerator = generator ?? throw new ArgumentNullException(nameof(generator));
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder WithJudge(IChatClient judge)
    {
        this.judge = judge ?? throw new ArgumentNullException(nameof(judge));
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder ShouldBeSemanticallySimilarTo(string expected, double minScore = 0.8)
    {
        if (string.IsNullOrWhiteSpace(expected))
        {
            throw new ArgumentException("Expected text cannot be null or whitespace.", nameof(expected));
        }

        if (minScore < -1d || minScore > 1d)
        {
            throw new ArgumentException("Minimum similarity score must be between -1.0 and 1.0.", nameof(minScore));
        }

        GetCurrentStep().SemanticExpectations.Add(new SemanticExpectation
        {
            Expected = expected,
            MinScore = minScore,
        });
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder ShouldSatisfy(string criteria)
    {
        if (string.IsNullOrWhiteSpace(criteria))
        {
            throw new ArgumentException("Criteria cannot be null or whitespace.", nameof(criteria));
        }

        GetCurrentStep().JudgeExpectations.Add(new JudgeExpectation { Criteria = criteria });
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder ShouldRespondWithin(TimeSpan maxLatency)
    {
        if (maxLatency <= TimeSpan.Zero)
        {
            throw new ArgumentException("Max latency must be greater than zero.", nameof(maxLatency));
        }

        this.maxLatency = maxLatency;
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder ShouldUseTokensUnder(int maxTokens)
    {
        if (maxTokens <= 0)
        {
            throw new ArgumentException("Max tokens must be greater than zero.", nameof(maxTokens));
        }

        this.maxTotalTokens = maxTokens;
        return this;
    }

    /// <inheritdoc/>
    public IDetesterBuilder ShouldUseCompletionTokensUnder(int maxTokens)
    {
        if (maxTokens <= 0)
        {
            throw new ArgumentException("Max tokens must be greater than zero.", nameof(maxTokens));
        }

        this.maxCompletionTokens = maxTokens;
        return this;
    }

    /// <inheritdoc/>
    public async Task AssertAsync(CancellationToken cancellationToken = default)
    {
        await AssertAsync(mainChatOptions, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task AssertAsync(ChatOptions? chatOptions, CancellationToken cancellationToken = default)
    {
        var result = await EvaluateInternalAsync(chatOptions ?? mainChatOptions, cancellationToken);

        if (!result.Passed)
        {
            var failed = result.Prompts
                .SelectMany(p => p.Assertions)
                .Where(a => !a.Passed)
                .ToList();

            var message = string.Join(Environment.NewLine, failed.Select(a => a.FailureMessage));
            var inner = failed.Select(a => a.Exception).FirstOrDefault(e => e is not null);

            throw inner is null
                ? new DetesterException(message)
                : new DetesterException(message, inner);
        }
    }

    /// <inheritdoc/>
    public Task<EvaluationResult> EvaluateAsync(CancellationToken cancellationToken = default)
    {
        return EvaluateInternalAsync(mainChatOptions, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<EvaluationResult> EvaluateAsync(ChatOptions? chatOptions, CancellationToken cancellationToken = default)
    {
        return EvaluateInternalAsync(chatOptions ?? mainChatOptions, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ReliabilityResult> AssertReliablyAsync(int runs, double requiredPassRate, CancellationToken cancellationToken = default)
    {
        if (runs <= 0)
        {
            throw new ArgumentException("Runs must be greater than zero.", nameof(runs));
        }

        if (requiredPassRate < 0 || requiredPassRate > 1)
        {
            throw new ArgumentException("Required pass rate must be between 0.0 and 1.0.", nameof(requiredPassRate));
        }

        var failures = new List<string>();
        var passCount = 0;

        for (var i = 0; i < runs; i++)
        {
            bool passed;
            IReadOnlyList<string> runFailures;

            try
            {
                var result = await EvaluateInternalAsync(mainChatOptions, cancellationToken);
                passed = result.Passed;
                runFailures = result.Failures;
            }
            catch (DetesterException ex)
            {
                passed = false;
                runFailures = [ex.Message];
            }

            if (passed)
            {
                passCount++;
            }
            else
            {
                var joined = string.Join(" | ", runFailures);
                failures.Add($"Run {i + 1}: {joined}");
            }
        }

        var passRate = (double)passCount / runs;
        var reliabilityResult = new ReliabilityResult(passCount, runs - passCount, passRate, failures);

        if (passRate < requiredPassRate)
        {
            var joinedFailures = string.Join(Environment.NewLine, failures);
            throw new DetesterException(
                $"Reliability check failed: {passCount}/{runs} runs passed ({passRate:P0}), " +
                $"but {requiredPassRate:P0} was required. Failures:{Environment.NewLine}{joinedFailures}");
        }

        return reliabilityResult;
    }

    private static string ReadTextFile(string filePath)
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

        return content;
    }

    private static bool ParametersMatch(
        IDictionary<string, object?>? actual,
        IDictionary<string, object?> expected)
    {
        if (actual == null)
        {
            return false;
        }

        if (actual.Count == 1 && actual.ContainsKey("request"))
        {
            var element = JsonSerializer.SerializeToElement(actual["request"]);
            foreach (var kvp in expected)
            {
                if (!element.TryGetProperty(kvp.Key, out var prop))
                {
                    return false;
                }

                var expectedJson = JsonSerializer.Serialize(kvp.Value);
                var actualJson = prop.GetRawText();

                if (!string.Equals(expectedJson, actualJson, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        if (actual.Count != expected.Count)
        {
            return false;
        }

        foreach (var kvp in expected)
        {
            if (!actual.TryGetValue(kvp.Key, out var actualValue))
            {
                return false;
            }

            if (!Equals(actualValue, kvp.Value))
            {
                return false;
            }
        }

        return true;
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length || a.Length == 0)
        {
            return 0d;
        }

        double dot = 0d;
        double normA = 0d;
        double normB = 0d;

        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * (double)b[i];
            normA += a[i] * (double)a[i];
            normB += b[i] * (double)b[i];
        }

        if (normA == 0d || normB == 0d)
        {
            return 0d;
        }

        return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    private static AssertionOutcome Pass(string description)
    {
        return new AssertionOutcome { Description = description, Passed = true };
    }

    private static AssertionOutcome Fail(string description, string message)
    {
        return new AssertionOutcome { Description = description, Passed = false, FailureMessage = message };
    }

    private static AssertionOutcome Fail(string description, string message, Exception exception)
    {
        return new AssertionOutcome
        {
            Description = description,
            Passed = false,
            FailureMessage = message,
            Exception = exception,
        };
    }

    private static void EvaluateTextAssertions(PromptStep step, string responseText, List<AssertionOutcome> outcomes)
    {
        if (step.ExpectedResponses.Count > 0)
        {
            var missing = step.ExpectedResponses
                .Where(expected => !responseText.Contains(expected, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (missing.Count > 0)
            {
                var missingText = string.Join(", ", missing.Select(e => $"'{e}'"));
                var message = $"Response did not contain expected text(s): {missingText}. Actual response: {responseText}";
                outcomes.Add(Fail("ShouldContainResponse", message));
            }
            else
            {
                outcomes.Add(Pass("ShouldContainResponse"));
            }
        }

        if (step.UnexpectedResponses.Count > 0)
        {
            var violating = step.UnexpectedResponses
                .Where(unexpected => responseText.Contains(unexpected, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (violating.Count > 0)
            {
                var violatingText = string.Join(", ", violating.Select(e => $"'{e}'"));
                var message = $"Response contained unexpected text(s): {violatingText}. Actual response: {responseText}";
                outcomes.Add(Fail("ShouldNotContainResponse", message));
            }
            else
            {
                outcomes.Add(Pass("ShouldNotContainResponse"));
            }
        }

        if (step.UnexpectedAnyResponses.Count > 0)
        {
            var violatingAny = step.UnexpectedAnyResponses
                .Where(unexpected => responseText.Contains(unexpected, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (violatingAny.Count > 0)
            {
                var violatingText = string.Join(", ", violatingAny.Select(e => $"'{e}'"));
                var message = $"Response contained one or more texts that should not appear: {violatingText}. Actual response: {responseText}";
                outcomes.Add(Fail("ShouldNotContainAnyResponse", message));
            }
            else
            {
                outcomes.Add(Pass("ShouldNotContainAnyResponse"));
            }
        }

        foreach (var pattern in step.RegexPatterns)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(responseText, pattern))
            {
                var message = $"Response did not match the required regular expression pattern '{pattern}'. Actual response: {responseText}";
                outcomes.Add(Fail("ShouldMatchRegex", message));
            }
            else
            {
                outcomes.Add(Pass("ShouldMatchRegex"));
            }
        }

        if (step.ContainAllSubstrings.Count > 0)
        {
            var missingAll = step.ContainAllSubstrings
                .Where(s => !responseText.Contains(s, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (missingAll.Count > 0)
            {
                var missingText = string.Join(", ", missingAll.Select(e => $"'{e}'"));
                var message = $"Response did not contain all required substrings: {missingText}. Actual response: {responseText}";
                outcomes.Add(Fail("ShouldContainAll", message));
            }
            else
            {
                outcomes.Add(Pass("ShouldContainAll"));
            }
        }

        foreach (var group in step.ContainAnyGroups)
        {
            var hasAny = group.Any(s => responseText.Contains(s, StringComparison.OrdinalIgnoreCase));
            if (!hasAny)
            {
                var options = string.Join("' OR '", group);
                var message = $"Response did not contain any of the required alternatives: '{options}'. Actual response: {responseText}";
                outcomes.Add(Fail("ShouldContainAny", message));
            }
            else
            {
                outcomes.Add(Pass("ShouldContainAny"));
            }
        }

        foreach (var expectation in step.EqualityExpectations)
        {
            if (!string.Equals(responseText, expectation.Expected, expectation.Comparison))
            {
                var message = $"Response was not equal to the expected text. Expected: '{expectation.Expected}', Actual: '{responseText}'.";
                outcomes.Add(Fail("ShouldBeEqualTo", message));
            }
            else
            {
                outcomes.Add(Pass("ShouldBeEqualTo"));
            }
        }

        foreach (var orGroup in step.OrResponseGroups)
        {
            var hasMatch = orGroup.Any(expected =>
                responseText.Contains(expected, StringComparison.OrdinalIgnoreCase));

            if (!hasMatch)
            {
                var orOptions = string.Join("' OR '", orGroup);
                var message = $"Response did not contain any of the expected alternatives: '{orOptions}'. Actual response: {responseText}";
                outcomes.Add(Fail("OrShouldContainResponse", message));
            }
            else
            {
                outcomes.Add(Pass("OrShouldContainResponse"));
            }
        }
    }

    private static void EvaluateFunctionCalls(PromptStep step, ChatResponse response, List<AssertionOutcome> outcomes)
    {
        if (step.ExpectedFunctionCalls.Count == 0 && step.NotExpectedFunctionCalls.Count == 0)
        {
            return;
        }

        var allCalls = response.Messages
            .Where(m => m.Role == ChatRole.Assistant)
            .SelectMany(m => m.Contents.OfType<FunctionCallContent>())
            .ToList();

        var remaining = new List<FunctionCallContent>(allCalls);

        foreach (var expectation in step.ExpectedFunctionCalls)
        {
            FunctionCallContent? matchedCall = null;

            foreach (var functionCall in remaining)
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
                if (remaining.Count == 0)
                {
                    var message = $"Expected function '{expectation.FunctionName}' to be called, but no function calls were made.";
                    outcomes.Add(Fail("ShouldCallFunction", message));
                    continue;
                }

                var actualFunctions = string.Join(", ", remaining.Select(f => $"'{f.Name}'"));

                if (expectation.ExpectedParameters != null)
                {
                    var expectedParams = string.Join(", ", expectation.ExpectedParameters.Select(p => $"{p.Key}={p.Value}"));
                    var actualParamsList = remaining
                        .Where(f => f.Name == expectation.FunctionName)
                        .Select(f => f.Arguments != null
                            ? string.Join(", ", f.Arguments.Select(a => a.Value))
                            : "NO_PARAMETERS");
                    var actualParams = string.Join(", ", actualParamsList);
                    var message = $"Expected function '{expectation.FunctionName}' to be called with parameters ({expectedParams}), but it was not called with those parameters. Actual parameters were: {actualParams}";
                    outcomes.Add(Fail("ShouldCallFunctionWithParameters", message));
                }
                else
                {
                    var message = $"Expected function '{expectation.FunctionName}' to be called, but only these functions were called: {actualFunctions}";
                    outcomes.Add(Fail("ShouldCallFunction", message));
                }

                continue;
            }

            remaining.Remove(matchedCall);
            var passDescription = expectation.ExpectedParameters != null
                ? "ShouldCallFunctionWithParameters"
                : "ShouldCallFunction";
            outcomes.Add(Pass(passDescription));
        }

        foreach (var notExpected in step.NotExpectedFunctionCalls)
        {
            if (allCalls.Any(f => f.Name == notExpected))
            {
                var message = $"Expected function '{notExpected}' NOT to be called, but it was called.";
                outcomes.Add(Fail("ShouldNotCallFunction", message));
            }
            else
            {
                outcomes.Add(Pass("ShouldNotCallFunction"));
            }
        }
    }

    private static void EvaluateJson(PromptStep step, string responseText, List<AssertionOutcome> outcomes)
    {
        foreach (var expectation in step.JsonExpectations)
        {
            try
            {
                var deserializedObject = JsonSerializer.Deserialize(responseText, expectation.TargetType, expectation.Options);

                if (deserializedObject == null)
                {
                    var message = $"Failed to deserialize JSON response to type '{expectation.TargetType.Name}': Deserialization resulted in null. Response: {responseText}";
                    outcomes.Add(Fail("ShouldHaveJsonOfType", message));
                    continue;
                }

                if (expectation.Validator != null)
                {
                    var validatorResult = expectation.Validator.DynamicInvoke(deserializedObject);
                    if (validatorResult == null)
                    {
                        var message = $"JSON response validation returned null for type '{expectation.TargetType.Name}'. The validation predicate must return a boolean value. Response: {responseText}";
                        outcomes.Add(Fail("ShouldHaveJsonOfType", message));
                        continue;
                    }

                    if (!(bool)validatorResult)
                    {
                        var message = $"JSON response validation failed for type '{expectation.TargetType.Name}'. The deserialized object did not pass the validation predicate. Response: {responseText}";
                        outcomes.Add(Fail("ShouldHaveJsonOfType", message));
                        continue;
                    }
                }

                outcomes.Add(Pass("ShouldHaveJsonOfType"));
            }
            catch (JsonException ex)
            {
                var message = $"Failed to deserialize JSON response to type '{expectation.TargetType.Name}': {ex.Message}. Response: {responseText}";
                outcomes.Add(Fail("ShouldHaveJsonOfType", message, ex));
            }
        }
    }

    private PromptStep GetCurrentStep()
    {
        return steps.Count > 0 ? steps[steps.Count - 1] : (pendingStep ??= new PromptStep());
    }

    private void AddPromptStep(string prompt)
    {
        if (pendingStep is not null && steps.Count == 0)
        {
            pendingStep.Prompt = prompt;
            steps.Add(pendingStep);
            pendingStep = null;
        }
        else
        {
            steps.Add(new PromptStep { Prompt = prompt });
        }
    }

    private async Task<EvaluationResult> EvaluateInternalAsync(ChatOptions? chatOptions, CancellationToken cancellationToken)
    {
        if (steps.Count == 0)
        {
            throw new DetesterException("No prompts have been added. Use WithPrompt or WithPrompts before asserting.");
        }

        if (embeddingGenerator == null && steps.Any(s => s.SemanticExpectations.Count > 0))
        {
            throw new InvalidOperationException(
                "ShouldBeSemanticallySimilarTo requires an embedding generator. Call WithEmbeddingGenerator(...) before evaluating.");
        }

        var chatHistory = new List<ChatMessage>();

        if (!string.IsNullOrWhiteSpace(instruction))
        {
            chatHistory.Add(new ChatMessage(ChatRole.System, instruction));
        }

        var promptEvaluations = new List<PromptEvaluation>();

        foreach (var step in steps)
        {
            chatHistory.Add(new ChatMessage(ChatRole.User, step.Prompt));

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var response = await chatClient.GetResponseAsync(chatHistory, chatOptions, cancellationToken: cancellationToken);
            sw.Stop();

            var outcomes = new List<AssertionOutcome>();

            if (response?.Text == null)
            {
                var nullMessage = $"Received null response for prompt: {step.Prompt}";
                outcomes.Add(Fail("Response", nullMessage));
                promptEvaluations.Add(new PromptEvaluation
                {
                    Prompt = step.Prompt,
                    ResponseText = string.Empty,
                    Assertions = outcomes,
                    Duration = sw.Elapsed,
                });
                continue;
            }

            var responseText = response.Text;

            if (maxLatency.HasValue)
            {
                if (sw.Elapsed > maxLatency.Value)
                {
                    var message = $"Response time {sw.Elapsed.TotalMilliseconds:F0}ms exceeded the maximum allowed latency of {maxLatency.Value.TotalMilliseconds:F0}ms for prompt: {step.Prompt}";
                    outcomes.Add(Fail("ShouldRespondWithin", message));
                }
                else
                {
                    outcomes.Add(Pass("ShouldRespondWithin"));
                }
            }

            if (maxTotalTokens.HasValue)
            {
                var totalTokens = response.Usage?.TotalTokenCount;
                if (totalTokens.HasValue && totalTokens.Value > maxTotalTokens.Value)
                {
                    var message = $"Total token usage {totalTokens.Value} exceeded the maximum of {maxTotalTokens.Value} for prompt: {step.Prompt}";
                    outcomes.Add(Fail("ShouldUseTokensUnder", message));
                }
                else
                {
                    outcomes.Add(Pass("ShouldUseTokensUnder"));
                }
            }

            if (maxCompletionTokens.HasValue)
            {
                var completionTokens = response.Usage?.OutputTokenCount;
                if (completionTokens.HasValue && completionTokens.Value > maxCompletionTokens.Value)
                {
                    var message = $"Completion token usage {completionTokens.Value} exceeded the maximum of {maxCompletionTokens.Value} for prompt: {step.Prompt}";
                    outcomes.Add(Fail("ShouldUseCompletionTokensUnder", message));
                }
                else
                {
                    outcomes.Add(Pass("ShouldUseCompletionTokensUnder"));
                }
            }

            EvaluateTextAssertions(step, responseText, outcomes);
            EvaluateFunctionCalls(step, response, outcomes);
            EvaluateJson(step, responseText, outcomes);
            await EvaluateSemanticAsync(step, responseText, outcomes, cancellationToken);
            await EvaluateJudgeAsync(step, responseText, outcomes, cancellationToken);

            chatHistory.Add(new ChatMessage(ChatRole.Assistant, responseText));

            promptEvaluations.Add(new PromptEvaluation
            {
                Prompt = step.Prompt,
                ResponseText = responseText,
                Assertions = outcomes,
                Duration = sw.Elapsed,
                TotalTokenCount = response.Usage?.TotalTokenCount,
                OutputTokenCount = response.Usage?.OutputTokenCount,
            });
        }

        return new EvaluationResult { Prompts = promptEvaluations };
    }

    private async Task EvaluateSemanticAsync(
        PromptStep step,
        string responseText,
        List<AssertionOutcome> outcomes,
        CancellationToken cancellationToken)
    {
        if (step.SemanticExpectations.Count == 0 || embeddingGenerator == null)
        {
            return;
        }

        var responseEmbedding = await embeddingGenerator.GenerateAsync(
            responseText, cancellationToken: cancellationToken);
        var responseVector = responseEmbedding.Vector.ToArray();

        foreach (var expectation in step.SemanticExpectations)
        {
            var expectedEmbedding = await embeddingGenerator.GenerateAsync(
                expectation.Expected, cancellationToken: cancellationToken);
            var expectedVector = expectedEmbedding.Vector.ToArray();

            var score = CosineSimilarity(responseVector, expectedVector);

            if (score < expectation.MinScore)
            {
                var message = $"Response was not semantically similar enough to the expected text. Similarity score {score:F3} was below the required minimum of {expectation.MinScore:F3}. Expected: '{expectation.Expected}', Actual response: {responseText}";
                outcomes.Add(Fail("ShouldBeSemanticallySimilarTo", message));
            }
            else
            {
                outcomes.Add(Pass("ShouldBeSemanticallySimilarTo"));
            }
        }
    }

    private async Task EvaluateJudgeAsync(
        PromptStep step,
        string responseText,
        List<AssertionOutcome> outcomes,
        CancellationToken cancellationToken)
    {
        if (step.JudgeExpectations.Count == 0)
        {
            return;
        }

        var judgeClient = judge ?? chatClient;

        foreach (var expectation in step.JudgeExpectations)
        {
            var systemPrompt =
                "You are a strict evaluator for automated tests. Given a CRITERIA and an AI RESPONSE, " +
                "decide whether the response satisfies the criteria. Reply with exactly 'PASS' if it does, " +
                "or 'FAIL: <short reason>' if it does not. Do not output anything else.";
            var userPrompt = $"CRITERIA:\n{expectation.Criteria}\n\nAI RESPONSE:\n{responseText}";

            var judgeMessages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, systemPrompt),
                new ChatMessage(ChatRole.User, userPrompt),
            };

            var judgeResponse = await judgeClient.GetResponseAsync(judgeMessages, cancellationToken: cancellationToken);
            var verdict = judgeResponse?.Text?.Trim() ?? string.Empty;

            if (verdict.StartsWith("PASS", StringComparison.OrdinalIgnoreCase))
            {
                outcomes.Add(Pass("ShouldSatisfy"));
            }
            else
            {
                var message = $"Response did not satisfy the criteria '{expectation.Criteria}'. Judge verdict: {verdict}. Actual response: {responseText}";
                outcomes.Add(Fail("ShouldSatisfy", message));
            }
        }
    }
}
