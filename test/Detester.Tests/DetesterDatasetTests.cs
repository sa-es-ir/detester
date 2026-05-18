// Copyright (c) Detester. All rights reserved.

namespace Detester.Tests;

using Detester.Abstraction;
using Microsoft.Extensions.AI;

/// <summary>
/// Tests for table-driven dataset evaluation.
/// </summary>
public class DetesterDatasetTests
{
    [Fact]
    public async Task EvaluateAsync_AllCasesPass_ReturnsFullPassRate()
    {
        var cases = new[]
        {
            new DatasetCase { Input = "Capital of France?", Expected = "Paris", Name = "fr" },
            new DatasetCase { Input = "Capital of Germany?", Expected = "Berlin", Name = "de" },
        };

        var result = await DetesterDataset.EvaluateAsync(
            EchoExpectedClient(),
            cases,
            (builder, c) => builder.WithPrompt(c.Input).ShouldContainResponse(c.Expected!),
            TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Cases.Count);
        Assert.Equal(2, result.PassCount);
        Assert.Equal(0, result.FailCount);
        Assert.Equal(1.0, result.PassRate);
    }

    [Fact]
    public async Task EvaluateAsync_SomeCasesFail_ReportsPerCase()
    {
        var cases = new[]
        {
            new DatasetCase { Input = "Capital of France?", Expected = "Paris" },
            new DatasetCase { Input = "Capital of Germany?", Expected = "Tokyo" },
        };

        var result = await DetesterDataset.EvaluateAsync(
            EchoExpectedClient(),
            cases,
            (builder, c) => builder.WithPrompt(c.Input).ShouldContainResponse(c.Expected!),
            TestContext.Current.CancellationToken);

        Assert.Equal(1, result.PassCount);
        Assert.Equal(1, result.FailCount);
        Assert.Equal(0.5, result.PassRate);
        Assert.True(result.Cases[0].Passed);
        Assert.False(result.Cases[1].Passed);
        Assert.NotNull(result.Cases[1].Evaluation);
    }

    [Fact]
    public async Task AssertAsync_BelowThreshold_Throws()
    {
        var cases = new[]
        {
            new DatasetCase { Input = "Capital of France?", Expected = "Paris" },
            new DatasetCase { Input = "Capital of Germany?", Expected = "Tokyo" },
        };

        var ex = await Assert.ThrowsAsync<DetesterException>(() =>
            DetesterDataset.AssertAsync(
                EchoExpectedClient(),
                cases,
                (builder, c) => builder.WithPrompt(c.Input).ShouldContainResponse(c.Expected!),
                requiredPassRate: 1.0,
                TestContext.Current.CancellationToken));

        Assert.Contains("Dataset check failed", ex.Message);
        Assert.Contains("1/2 cases passed", ex.Message);
    }

    [Fact]
    public async Task AssertAsync_MeetsThreshold_ReturnsResult()
    {
        var cases = new[]
        {
            new DatasetCase { Input = "Capital of France?", Expected = "Paris" },
            new DatasetCase { Input = "Capital of Germany?", Expected = "Berlin" },
        };

        var result = await DetesterDataset.AssertAsync(
            EchoExpectedClient(),
            cases,
            (builder, c) => builder.WithPrompt(c.Input).ShouldContainResponse(c.Expected!),
            requiredPassRate: 1.0,
            TestContext.Current.CancellationToken);

        Assert.Equal(1.0, result.PassRate);
    }

    private static CallbackMockChatClient EchoExpectedClient()
    {
        return new CallbackMockChatClient(messages =>
        {
            var lastUser = messages.Last(m => m.Role == ChatRole.User).Text ?? string.Empty;
            if (lastUser.Contains("France", StringComparison.OrdinalIgnoreCase))
            {
                return "Paris";
            }

            if (lastUser.Contains("Germany", StringComparison.OrdinalIgnoreCase))
            {
                return "Berlin";
            }

            return "unknown";
        });
    }
}
