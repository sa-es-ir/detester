// Copyright (c) Detester. All rights reserved.

namespace Detester.Abstraction;

/// <summary>
/// Represents the outcome of a single assertion evaluated against an AI response.
/// </summary>
public sealed record AssertionOutcome
{
    /// <summary>
    /// Gets a short, human-readable description of the assertion that was evaluated.
    /// </summary>
    required public string Description { get; init; }

    /// <summary>
    /// Gets a value indicating whether the assertion passed.
    /// </summary>
    required public bool Passed { get; init; }

    /// <summary>
    /// Gets the failure message when the assertion did not pass; otherwise <see langword="null"/>.
    /// </summary>
    public string? FailureMessage { get; init; }

    /// <summary>
    /// Gets the underlying exception that caused the assertion to fail, if any
    /// (for example, the <see cref="System.Text.Json.JsonException"/> from a failed JSON deserialization).
    /// </summary>
    public Exception? Exception { get; init; }
}

/// <summary>
/// Represents the evaluation of a single prompt: the response it produced and the
/// outcomes of every assertion scoped to that prompt.
/// </summary>
public sealed record PromptEvaluation
{
    /// <summary>
    /// Gets the prompt text that was sent to the AI.
    /// </summary>
    required public string Prompt { get; init; }

    /// <summary>
    /// Gets the response text returned by the AI for this prompt.
    /// </summary>
    required public string ResponseText { get; init; }

    /// <summary>
    /// Gets the outcomes of every assertion scoped to this prompt.
    /// </summary>
    required public IReadOnlyList<AssertionOutcome> Assertions { get; init; }

    /// <summary>
    /// Gets the wall-clock time taken to produce the response for this prompt.
    /// </summary>
    required public TimeSpan Duration { get; init; }

    /// <summary>
    /// Gets the total token count reported by the provider, if available.
    /// </summary>
    public long? TotalTokenCount { get; init; }

    /// <summary>
    /// Gets the output (completion) token count reported by the provider, if available.
    /// </summary>
    public long? OutputTokenCount { get; init; }

    /// <summary>
    /// Gets a value indicating whether every assertion scoped to this prompt passed.
    /// </summary>
    public bool Passed => Assertions.All(a => a.Passed);
}

/// <summary>
/// Represents the full result of evaluating a Detester test without throwing,
/// including per-prompt responses and assertion outcomes.
/// </summary>
public sealed record EvaluationResult
{
    /// <summary>
    /// Gets the evaluation of each prompt, in execution order.
    /// </summary>
    required public IReadOnlyList<PromptEvaluation> Prompts { get; init; }

    /// <summary>
    /// Gets a value indicating whether every assertion across every prompt passed.
    /// </summary>
    public bool Passed => Prompts.Count > 0 && Prompts.All(p => p.Passed);

    /// <summary>
    /// Gets the failure messages for every assertion that did not pass, across all prompts.
    /// </summary>
    public IReadOnlyList<string> Failures =>
        Prompts
            .SelectMany(p => p.Assertions)
            .Where(a => !a.Passed && a.FailureMessage is not null)
            .Select(a => a.FailureMessage!)
            .ToList();
}
