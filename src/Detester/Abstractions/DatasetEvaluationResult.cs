// Copyright (c) Detester. All rights reserved.

namespace Detester.Abstraction;

/// <summary>
/// Represents a single input case in a dataset evaluation.
/// </summary>
public sealed record DatasetCase
{
    /// <summary>
    /// Gets the input/prompt for this case.
    /// </summary>
    required public string Input { get; init; }

    /// <summary>
    /// Gets the expected output for this case, if the test configuration uses it.
    /// </summary>
    public string? Expected { get; init; }

    /// <summary>
    /// Gets an optional human-readable name to identify this case in reports.
    /// </summary>
    public string? Name { get; init; }
}

/// <summary>
/// Represents the result of evaluating a single dataset case.
/// </summary>
public sealed record DatasetCaseResult
{
    /// <summary>
    /// Gets the case that was evaluated.
    /// </summary>
    required public DatasetCase Case { get; init; }

    /// <summary>
    /// Gets a value indicating whether the case passed all assertions.
    /// </summary>
    required public bool Passed { get; init; }

    /// <summary>
    /// Gets the detailed evaluation result for the case, if it was produced.
    /// </summary>
    public EvaluationResult? Evaluation { get; init; }

    /// <summary>
    /// Gets a configuration/error message when the case could not be evaluated.
    /// </summary>
    public string? Error { get; init; }
}

/// <summary>
/// Represents the aggregate result of evaluating a dataset of cases.
/// </summary>
public sealed record DatasetEvaluationResult
{
    /// <summary>
    /// Gets the per-case results, in input order.
    /// </summary>
    required public IReadOnlyList<DatasetCaseResult> Cases { get; init; }

    /// <summary>
    /// Gets the number of cases that passed.
    /// </summary>
    public int PassCount => Cases.Count(c => c.Passed);

    /// <summary>
    /// Gets the number of cases that failed.
    /// </summary>
    public int FailCount => Cases.Count - PassCount;

    /// <summary>
    /// Gets the ratio of passing cases to total cases (0.0 to 1.0).
    /// </summary>
    public double PassRate => Cases.Count == 0 ? 0d : (double)PassCount / Cases.Count;
}
