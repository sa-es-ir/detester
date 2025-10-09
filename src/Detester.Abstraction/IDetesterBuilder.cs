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
    /// Asserts that the AI response contains the specified text as an alternative to the previous assertion.
    /// This creates an OR condition where at least one of the options in the OR group must match.
    /// </summary>
    /// <param name="expectedText">The alternative text that should be present in the response.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IDetesterBuilder OrShouldContainResponse(string expectedText);

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
