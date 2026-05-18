// Copyright (c) Detester. All rights reserved.

namespace Detester.Tests;

using Detester.Abstraction;
using Microsoft.Extensions.AI;

/// <summary>
/// Tests for the non-throwing EvaluateAsync API and structured result shape.
/// </summary>
public class DetesterBuilderEvaluateTests
{
    [Fact]
    public async Task EvaluateAsync_AllAssertionsPass_ReturnsPassedResult()
    {
        var mockClient = new MockChatClient { ResponseText = "The answer is 42 and helpful" };
        var builder = new DetesterBuilder(mockClient);

        var result = await builder
            .WithPrompt("Question")
            .ShouldContainResponse("42")
            .ShouldContainResponse("helpful")
            .EvaluateAsync(TestContext.Current.CancellationToken);

        Assert.True(result.Passed);
        Assert.Single(result.Prompts);
        Assert.Empty(result.Failures);
        Assert.All(result.Prompts[0].Assertions, a => Assert.True(a.Passed));
    }

    [Fact]
    public async Task EvaluateAsync_DoesNotThrowOnAssertionFailure()
    {
        var mockClient = new MockChatClient { ResponseText = "Nope" };
        var builder = new DetesterBuilder(mockClient);

        var result = await builder
            .WithPrompt("Question")
            .ShouldContainResponse("missing")
            .EvaluateAsync(TestContext.Current.CancellationToken);

        Assert.False(result.Passed);
        Assert.Single(result.Failures);
        Assert.Contains("did not contain expected", result.Failures[0]);
    }

    [Fact]
    public async Task EvaluateAsync_CapturesTokenAndDurationMetadata()
    {
        var mockClient = new MockChatClient
        {
            ResponseText = "Answer",
            UsageDetailsToReturn = new UsageDetails { TotalTokenCount = 50, OutputTokenCount = 20 },
        };
        var builder = new DetesterBuilder(mockClient);

        var result = await builder
            .WithPrompt("Question")
            .ShouldContainResponse("Answer")
            .EvaluateAsync(TestContext.Current.CancellationToken);

        var prompt = result.Prompts[0];
        Assert.Equal(50, prompt.TotalTokenCount);
        Assert.Equal(20, prompt.OutputTokenCount);
        Assert.True(prompt.Duration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task EvaluateAsync_WithoutPrompts_ThrowsConfigurationError()
    {
        var builder = new DetesterBuilder(new MockChatClient());

        await Assert.ThrowsAsync<DetesterException>(() =>
            builder.EvaluateAsync(TestContext.Current.CancellationToken));
    }
}
