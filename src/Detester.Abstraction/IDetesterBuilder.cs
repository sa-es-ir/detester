namespace Detester.Abstraction;

/// <summary>
/// Defines the contract for building deterministic AI tests.
/// </summary>
public interface IDetesterBuilder
{
    /// <summary>
    /// Adds a single prompt to the test execution.
    /// </summary>
    /// <param name="prompt">The prompt to send to the AI.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IDetesterBuilder WithPrompt(string prompt);

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
    /// Asserts the test asynchronously by executing the configured prompts and validating responses.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AssertAsync(CancellationToken cancellationToken = default);
}
