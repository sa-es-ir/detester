// Copyright (c) Detester. All rights reserved.

namespace Detester.Abstraction;

/// <summary>
/// Represents the result of a reliability evaluation that runs a test multiple times
/// to verify consistent behavior across repeated executions.
/// </summary>
/// <param name="PassCount">The number of runs that passed all assertions.</param>
/// <param name="FailCount">The number of runs that failed at least one assertion.</param>
/// <param name="PassRate">The ratio of passing runs to total runs (0.0 to 1.0).</param>
/// <param name="Failures">The failure messages collected from each failed run.</param>
public sealed record ReliabilityResult(
    int PassCount,
    int FailCount,
    double PassRate,
    IReadOnlyList<string> Failures)
{
    /// <summary>
    /// Gets the total number of runs executed.
    /// </summary>
    public int TotalRuns => PassCount + FailCount;
}
