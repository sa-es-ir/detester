namespace Detester.Abstraction;

/// <summary>
/// Defines the contract for building deterministic AI tests.
/// </summary>
public interface IDetesterBuilder
{
    /// <summary>
    /// Sets the instruction (system message) for the AI model.
    /// </summary>
    /// <param name="instruction">The instruction to guide the AI model's behavior.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IDetesterBuilder WithInstruction(string instruction);

    /// <summary>
    /// Sets the instruction (system message) for the AI model from a file.
    /// Accepts markdown (.md) and text (.txt) files.
    /// </summary>
    /// <param name="filePath">The path to the file containing the instruction.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IDetesterBuilder WithInstructionFromFile(string filePath);

    /// <summary>
    /// Adds a single prompt to the test execution.
    /// </summary>
    /// <param name="prompt">The prompt to send to the AI.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IDetesterBuilder WithPrompt(string prompt);

    /// <summary>
    /// Adds a single prompt to the test execution from a file.
    /// Accepts markdown (.md) and text (.txt) files.
    /// </summary>
    /// <param name="filePath">The path to the file containing the prompt.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IDetesterBuilder WithPromptFromFile(string filePath);

    /// <summary>
    /// Adds multiple prompts to the test execution.
    /// </summary>
    /// <param name="prompts">The prompts to send to the AI.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IDetesterBuilder WithPrompts(params string[] prompts);

    /// <summary>
    /// Asserts that the AI response contains the specified text.
    /// </summary>
    /// <param name="expectedText">The text that should be present in the response.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IDetesterBuilder ShouldContainResponse(string expectedText);

    /// <summary>
    /// Asserts that the AI response does not contain the specified text.
    /// </summary>
    /// <param name="unexpectedText">The text that should not be present in the response.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IDetesterBuilder ShouldNotContainResponse(string unexpectedText);

    /// <summary>
    /// Asserts that the AI response does not contain any of the specified texts.
    /// This ensures that none of the provided texts appear in the response and is
    /// equivalent to calling <see cref="ShouldNotContainResponse(string)"/> once
    /// for each value in <paramref name="unexpectedTexts"/>.
    /// </summary>
    /// <param name="unexpectedTexts">The texts that must not be present anywhere in the response.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IDetesterBuilder ShouldNotContainAnyResponse(params string[] unexpectedTexts);

    /// <summary>
    /// Asserts that the AI response matches the specified regular expression pattern.
    /// </summary>
    /// <param name="pattern">The regular expression pattern the response must match.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IDetesterBuilder ShouldMatchRegex(string pattern);

    /// <summary>
    /// Asserts that the AI response does not contain the specified text.
    /// Alias for <see cref="ShouldNotContainResponse"/> for semantic clarity.
    /// </summary>
    /// <param name="unexpectedText">The text that should not be present in the response.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IDetesterBuilder ShouldNotContain(string unexpectedText);

    /// <summary>
    /// Asserts that the AI response contains all of the specified substrings.
    /// </summary>
    /// <param name="expectedSubstrings">The substrings that must all be present in the response.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IDetesterBuilder ShouldContainAll(params string[] expectedSubstrings);

    /// <summary>
    /// Asserts that the AI response contains at least one of the specified substrings.
    /// </summary>
    /// <param name="expectedSubstrings">The substrings where at least one must be present in the response.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IDetesterBuilder ShouldContainAny(params string[] expectedSubstrings);

    /// <summary>
    /// Asserts that the AI response is equal to the specified text using the provided string comparison.
    /// By default, this uses a case-insensitive comparison for consistency with other string assertion methods.
    /// </summary>
    /// <param name="expected">The expected response text.</param>
    /// <param name="comparison">
    /// The string comparison to use when comparing the response to the expected text.
    /// Defaults to <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </param>
    /// <returns>The builder instance for method chaining.</returns>
    IDetesterBuilder ShouldBeEqualTo(string expected, StringComparison comparison = StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Asserts that the AI response contains the specified text as an alternative to the previous assertion.
    /// This creates an OR condition where at least one of the options in the OR group must match.
    /// </summary>
    /// <param name="expectedText">The alternative text that should be present in the response.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IDetesterBuilder OrShouldContainResponse(string expectedText);

    /// <summary>
    /// Asserts that the AI response contains valid JSON that can be deserialized to the specified type.
    /// Optionally validates the deserialized object using a predicate function.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON response into.</typeparam>
    /// <param name="options">JSON serializer options to use for deserialization. If null, default options are used.</param>
    /// <param name="validator">Optional predicate to validate the deserialized object. Returns true if validation passes.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IDetesterBuilder ShouldHaveJsonOfType<T>(System.Text.Json.JsonSerializerOptions? options = null, Func<T, bool>? validator = null);

    /// <summary>
    /// Asserts that the AI model called the specified function/tool.
    /// </summary>
    /// <param name="functionName">The name of the function that should have been called.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IDetesterBuilder ShouldCallFunction(string functionName);

    /// <summary>
    /// Asserts that the AI model called the specified function/tool with the expected parameters.
    /// </summary>
    /// <param name="functionName">The name of the function that should have been called.</param>
    /// <param name="expectedParameters">The expected parameters that should have been passed to the function.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IDetesterBuilder ShouldCallFunctionWithParameters(string functionName, IDictionary<string, object?> expectedParameters);

    /// <summary>
    /// Asserts the test asynchronously by executing the configured prompts and validating responses.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AssertAsync(CancellationToken cancellationToken = default);
}
