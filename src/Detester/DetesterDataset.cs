namespace Detester;

using Detester.Abstraction;
using Microsoft.Extensions.AI;

/// <summary>
/// Runs a Detester test configuration over a dataset of input cases (table-driven evaluation).
/// </summary>
public static class DetesterDataset
{
    /// <summary>
    /// Evaluates every case in <paramref name="cases"/> against a fresh builder configured by
    /// <paramref name="configure"/>, returning per-case results and an aggregate pass rate.
    /// Cases are evaluated without throwing on assertion failures.
    /// </summary>
    /// <param name="chatClient">The chat client used for every case.</param>
    /// <param name="cases">The dataset of input cases.</param>
    /// <param name="configure">
    /// A callback that configures the builder for each case (e.g. adds the prompt and assertions).
    /// </param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The aggregate dataset evaluation result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when a required argument is null.</exception>
    public static async Task<DatasetEvaluationResult> EvaluateAsync(
        IChatClient chatClient,
        IEnumerable<DatasetCase> cases,
        Action<IDetesterBuilder, DatasetCase> configure,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chatClient);
        ArgumentNullException.ThrowIfNull(cases);
        ArgumentNullException.ThrowIfNull(configure);

        var results = new List<DatasetCaseResult>();

        foreach (var datasetCase in cases)
        {
            var builder = new DetesterBuilder(chatClient);
            configure(builder, datasetCase);

            try
            {
                var evaluation = await builder.EvaluateAsync(cancellationToken);
                results.Add(new DatasetCaseResult
                {
                    Case = datasetCase,
                    Passed = evaluation.Passed,
                    Evaluation = evaluation,
                });
            }
            catch (DetesterException ex)
            {
                results.Add(new DatasetCaseResult
                {
                    Case = datasetCase,
                    Passed = false,
                    Error = ex.Message,
                });
            }
            catch (InvalidOperationException ex)
            {
                results.Add(new DatasetCaseResult
                {
                    Case = datasetCase,
                    Passed = false,
                    Error = ex.Message,
                });
            }
        }

        return new DatasetEvaluationResult { Cases = results };
    }

    /// <summary>
    /// Evaluates the dataset and throws <see cref="DetesterException"/> if the pass rate
    /// is below <paramref name="requiredPassRate"/>.
    /// </summary>
    /// <param name="chatClient">The chat client used for every case.</param>
    /// <param name="cases">The dataset of input cases.</param>
    /// <param name="configure">A callback that configures the builder for each case.</param>
    /// <param name="requiredPassRate">The minimum fraction of cases that must pass (0.0 to 1.0).</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The aggregate dataset evaluation result.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="requiredPassRate"/> is out of range.</exception>
    /// <exception cref="DetesterException">Thrown when the pass rate is below the required threshold.</exception>
    public static async Task<DatasetEvaluationResult> AssertAsync(
        IChatClient chatClient,
        IEnumerable<DatasetCase> cases,
        Action<IDetesterBuilder, DatasetCase> configure,
        double requiredPassRate,
        CancellationToken cancellationToken = default)
    {
        if (requiredPassRate < 0 || requiredPassRate > 1)
        {
            throw new ArgumentException("Required pass rate must be between 0.0 and 1.0.", nameof(requiredPassRate));
        }

        var result = await EvaluateAsync(chatClient, cases, configure, cancellationToken);

        if (result.PassRate < requiredPassRate)
        {
            var failures = result.Cases
                .Where(c => !c.Passed)
                .Select(c =>
                {
                    var label = c.Case.Name ?? c.Case.Input;
                    var detail = c.Error ?? string.Join(" | ", c.Evaluation?.Failures ?? []);
                    return $"Case '{label}': {detail}";
                });

            throw new DetesterException(
                $"Dataset check failed: {result.PassCount}/{result.Cases.Count} cases passed " +
                $"({result.PassRate:P0}), but {requiredPassRate:P0} was required. " +
                $"Failures:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
        }

        return result;
    }
}
