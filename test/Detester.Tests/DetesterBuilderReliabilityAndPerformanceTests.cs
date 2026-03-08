// Copyright (c) Detester. All rights reserved.

namespace Detester.Tests;

using Detester.Abstraction;
using Microsoft.Extensions.AI;

/// <summary>
/// Tests for reliability evaluation, latency assertions, and token usage assertions.
/// </summary>
public class DetesterBuilderReliabilityAndPerformanceTests
{
    // -------------------------------------------------------------------------
    // ShouldRespondWithin – input validation
    // -------------------------------------------------------------------------

    [Fact]
    public void ShouldRespondWithin_WithZeroTimeSpan_ThrowsArgumentException()
    {
        var builder = new DetesterBuilder(new MockChatClient());

        Assert.Throws<ArgumentException>(() => builder.ShouldRespondWithin(TimeSpan.Zero));
    }

    [Fact]
    public void ShouldRespondWithin_WithNegativeTimeSpan_ThrowsArgumentException()
    {
        var builder = new DetesterBuilder(new MockChatClient());

        Assert.Throws<ArgumentException>(() => builder.ShouldRespondWithin(TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public void ShouldRespondWithin_ReturnsBuilderForChaining()
    {
        var builder = new DetesterBuilder(new MockChatClient());

        var result = builder.ShouldRespondWithin(TimeSpan.FromSeconds(5));

        Assert.Same(builder, result);
    }

    [Fact]
    public async Task ShouldRespondWithin_WhenResponseIsWithinLimit_DoesNotThrow()
    {
        var mockClient = new MockChatClient { ResponseText = "Fast response", ResponseDelayMs = 0 };
        var builder = new DetesterBuilder(mockClient);

        await builder
            .WithPrompt("Hello")
            .ShouldRespondWithin(TimeSpan.FromSeconds(10))
            .AssertAsync();
    }

    [Fact]
    public async Task ShouldRespondWithin_WhenResponseExceedsLimit_ThrowsDetesterException()
    {
        var mockClient = new MockChatClient { ResponseText = "Slow response", ResponseDelayMs = 200 };
        var builder = new DetesterBuilder(mockClient);

        await Assert.ThrowsAsync<DetesterException>(() =>
            builder
                .WithPrompt("Hello")
                .ShouldRespondWithin(TimeSpan.FromMilliseconds(50))
                .AssertAsync());
    }

    // -------------------------------------------------------------------------
    // ShouldUseTokensUnder – input validation
    // -------------------------------------------------------------------------

    [Fact]
    public void ShouldUseTokensUnder_WithZero_ThrowsArgumentException()
    {
        var builder = new DetesterBuilder(new MockChatClient());

        Assert.Throws<ArgumentException>(() => builder.ShouldUseTokensUnder(0));
    }

    [Fact]
    public void ShouldUseTokensUnder_WithNegative_ThrowsArgumentException()
    {
        var builder = new DetesterBuilder(new MockChatClient());

        Assert.Throws<ArgumentException>(() => builder.ShouldUseTokensUnder(-1));
    }

    [Fact]
    public void ShouldUseTokensUnder_ReturnsBuilderForChaining()
    {
        var builder = new DetesterBuilder(new MockChatClient());

        var result = builder.ShouldUseTokensUnder(500);

        Assert.Same(builder, result);
    }

    [Fact]
    public async Task ShouldUseTokensUnder_WhenUsageNotReturned_DoesNotThrow()
    {
        var mockClient = new MockChatClient { ResponseText = "Answer", UsageDetailsToReturn = null };
        var builder = new DetesterBuilder(mockClient);

        await builder
            .WithPrompt("Hello")
            .ShouldUseTokensUnder(100)
            .AssertAsync();
    }

    [Fact]
    public async Task ShouldUseTokensUnder_WhenUnderLimit_DoesNotThrow()
    {
        var mockClient = new MockChatClient
        {
            ResponseText = "Answer",
            UsageDetailsToReturn = new UsageDetails { TotalTokenCount = 50 },
        };

        await new DetesterBuilder(mockClient)
            .WithPrompt("Hello")
            .ShouldUseTokensUnder(100)
            .AssertAsync();
    }

    [Fact]
    public async Task ShouldUseTokensUnder_WhenOverLimit_ThrowsDetesterException()
    {
        var mockClient = new MockChatClient
        {
            ResponseText = "Answer",
            UsageDetailsToReturn = new UsageDetails { TotalTokenCount = 200 },
        };

        await Assert.ThrowsAsync<DetesterException>(() =>
            new DetesterBuilder(mockClient)
                .WithPrompt("Hello")
                .ShouldUseTokensUnder(100)
                .AssertAsync());
    }

    // -------------------------------------------------------------------------
    // ShouldUseCompletionTokensUnder – input validation
    // -------------------------------------------------------------------------

    [Fact]
    public void ShouldUseCompletionTokensUnder_WithZero_ThrowsArgumentException()
    {
        var builder = new DetesterBuilder(new MockChatClient());

        Assert.Throws<ArgumentException>(() => builder.ShouldUseCompletionTokensUnder(0));
    }

    [Fact]
    public void ShouldUseCompletionTokensUnder_WithNegative_ThrowsArgumentException()
    {
        var builder = new DetesterBuilder(new MockChatClient());

        Assert.Throws<ArgumentException>(() => builder.ShouldUseCompletionTokensUnder(-1));
    }

    [Fact]
    public void ShouldUseCompletionTokensUnder_ReturnsBuilderForChaining()
    {
        var builder = new DetesterBuilder(new MockChatClient());

        var result = builder.ShouldUseCompletionTokensUnder(200);

        Assert.Same(builder, result);
    }

    [Fact]
    public async Task ShouldUseCompletionTokensUnder_WhenUnderLimit_DoesNotThrow()
    {
        var mockClient = new MockChatClient
        {
            ResponseText = "Answer",
            UsageDetailsToReturn = new UsageDetails { OutputTokenCount = 40 },
        };

        await new DetesterBuilder(mockClient)
            .WithPrompt("Hello")
            .ShouldUseCompletionTokensUnder(100)
            .AssertAsync();
    }

    [Fact]
    public async Task ShouldUseCompletionTokensUnder_WhenOverLimit_ThrowsDetesterException()
    {
        var mockClient = new MockChatClient
        {
            ResponseText = "Answer",
            UsageDetailsToReturn = new UsageDetails { OutputTokenCount = 150 },
        };

        await Assert.ThrowsAsync<DetesterException>(() =>
            new DetesterBuilder(mockClient)
                .WithPrompt("Hello")
                .ShouldUseCompletionTokensUnder(100)
                .AssertAsync());
    }

    // -------------------------------------------------------------------------
    // AssertReliablyAsync – input validation
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AssertReliablyAsync_WithZeroRuns_ThrowsArgumentException()
    {
        var builder = new DetesterBuilder(new MockChatClient());
        builder.WithPrompt("Hello");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            builder.AssertReliablyAsync(runs: 0, requiredPassRate: 0.9));
    }

    [Fact]
    public async Task AssertReliablyAsync_WithNegativeRuns_ThrowsArgumentException()
    {
        var builder = new DetesterBuilder(new MockChatClient());
        builder.WithPrompt("Hello");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            builder.AssertReliablyAsync(runs: -1, requiredPassRate: 0.9));
    }

    [Fact]
    public async Task AssertReliablyAsync_WithPassRateAboveOne_ThrowsArgumentException()
    {
        var builder = new DetesterBuilder(new MockChatClient());
        builder.WithPrompt("Hello");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            builder.AssertReliablyAsync(runs: 5, requiredPassRate: 1.1));
    }

    [Fact]
    public async Task AssertReliablyAsync_WithNegativePassRate_ThrowsArgumentException()
    {
        var builder = new DetesterBuilder(new MockChatClient());
        builder.WithPrompt("Hello");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            builder.AssertReliablyAsync(runs: 5, requiredPassRate: -0.1));
    }

    // -------------------------------------------------------------------------
    // AssertReliablyAsync – behavior
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AssertReliablyAsync_WhenAllRunsPass_ReturnsFullPassRate()
    {
        var mockClient = new MockChatClient { ResponseText = "Paris is the capital of France" };

        var result = await new DetesterBuilder(mockClient)
            .WithPrompt("What is the capital of France?")
            .ShouldContainResponse("Paris")
            .AssertReliablyAsync(runs: 5, requiredPassRate: 1.0);

        Assert.Equal(5, result.PassCount);
        Assert.Equal(0, result.FailCount);
        Assert.Equal(5, result.TotalRuns);
        Assert.Equal(1.0, result.PassRate);
        Assert.Empty(result.Failures);
    }

    [Fact]
    public async Task AssertReliablyAsync_WhenAllRunsFail_ThrowsDetesterException()
    {
        var mockClient = new MockChatClient { ResponseText = "I don't know" };

        await Assert.ThrowsAsync<DetesterException>(() =>
            new DetesterBuilder(mockClient)
                .WithPrompt("What is the capital of France?")
                .ShouldContainResponse("Paris")
                .AssertReliablyAsync(runs: 3, requiredPassRate: 0.5));
    }

    [Fact]
    public async Task AssertReliablyAsync_WhenSomeRunsFailButPassRateMeetsThreshold_ReturnsResult()
    {
        var callCount = 0;
        var alternatingClient = new CallbackMockChatClient(messages =>
        {
            callCount++;
            return callCount % 2 == 0 ? "Paris is the capital" : "I don't know";
        });

        var result = await new DetesterBuilder(alternatingClient)
            .WithPrompt("Capital?")
            .ShouldContainResponse("Paris")
            .AssertReliablyAsync(runs: 4, requiredPassRate: 0.5);

        Assert.Equal(4, result.TotalRuns);
        Assert.Equal(2, result.PassCount);
        Assert.Equal(2, result.FailCount);
        Assert.Equal(0.5, result.PassRate);
        Assert.Equal(2, result.Failures.Count);
    }

    [Fact]
    public async Task AssertReliablyAsync_WhenBelowThreshold_ThrowsWithFailureDetails()
    {
        var mockClient = new MockChatClient { ResponseText = "I do not know" };

        var ex = await Assert.ThrowsAsync<DetesterException>(() =>
            new DetesterBuilder(mockClient)
                .WithPrompt("Capital?")
                .ShouldContainResponse("Paris")
                .AssertReliablyAsync(runs: 2, requiredPassRate: 0.5));

        Assert.Contains("0/2 runs passed", ex.Message);
    }

    [Fact]
    public async Task AssertReliablyAsync_FailuresList_ContainsRunNumbers()
    {
        var mockClient = new MockChatClient { ResponseText = "Not the answer" };
        DetesterException? caughtEx = null;

        try
        {
            await new DetesterBuilder(mockClient)
                .WithPrompt("Capital?")
                .ShouldContainResponse("Paris")
                .AssertReliablyAsync(runs: 3, requiredPassRate: 1.0);
        }
        catch (DetesterException ex)
        {
            caughtEx = ex;
        }

        Assert.NotNull(caughtEx);
        Assert.Contains("Run 1:", caughtEx.Message);
        Assert.Contains("Run 2:", caughtEx.Message);
        Assert.Contains("Run 3:", caughtEx.Message);
    }

    // -------------------------------------------------------------------------
    // ReliabilityResult – record properties
    // -------------------------------------------------------------------------

    [Fact]
    public void ReliabilityResult_TotalRuns_IsSumOfPassAndFail()
    {
        var result = new ReliabilityResult(7, 3, 0.7, []);

        Assert.Equal(10, result.TotalRuns);
    }

    [Fact]
    public void ReliabilityResult_WithNoFailures_HasEmptyFailureList()
    {
        var result = new ReliabilityResult(5, 0, 1.0, []);

        Assert.Empty(result.Failures);
    }
}
