// Copyright (c) Detester. All rights reserved.

namespace Detester.Tests;

using Detester.Abstraction;
using Microsoft.Extensions.AI;

/// <summary>
/// Tests proving assertions are scoped to the prompt they follow, not checked
/// against every response (the corrected per-prompt model).
/// </summary>
public class DetesterBuilderScopingTests
{
    [Fact]
    public async Task PerPromptAssertions_BindToTheirOwnPrompt_Passes()
    {
        var builder = new DetesterBuilder(CapitalsClient());

        await builder
            .WithPrompt("What is the capital of France?")
            .ShouldContainResponse("Paris")
            .WithPrompt("What is the capital of Germany?")
            .ShouldContainResponse("Berlin")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertionAfterFirstPrompt_NotCheckedAgainstSecondResponse()
    {
        // Under the old global model, "Paris" would also be checked against the
        // Germany response ("...Berlin.") and fail. Under the corrected model it passes.
        var builder = new DetesterBuilder(CapitalsClient());

        await builder
            .WithPrompt("What is the capital of France?")
            .ShouldContainResponse("Paris")
            .WithPrompt("What is the capital of Germany?")
            .ShouldContainResponse("Berlin")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertionScopedToWrongPrompt_Fails()
    {
        var builder = new DetesterBuilder(CapitalsClient());

        var ex = await Assert.ThrowsAsync<DetesterException>(() =>
            builder
                .WithPrompt("What is the capital of France?")
                .ShouldContainResponse("Berlin")
                .AssertAsync(TestContext.Current.CancellationToken));

        Assert.Contains("Berlin", ex.Message);
    }

    [Fact]
    public async Task AssertionsDeclaredBeforeAnyPrompt_BindToFirstPrompt()
    {
        var mockClient = new MockChatClient { ResponseText = "Hello helpful world" };
        var builder = new DetesterBuilder(mockClient);

        await builder
            .ShouldContainResponse("helpful")
            .WithPrompt("Say hello")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EvaluateAsync_ReportsPerPromptOutcomes()
    {
        var builder = new DetesterBuilder(CapitalsClient());

        var result = await builder
            .WithPrompt("What is the capital of France?")
            .ShouldContainResponse("Paris")
            .WithPrompt("What is the capital of Germany?")
            .ShouldContainResponse("Tokyo")
            .EvaluateAsync(TestContext.Current.CancellationToken);

        Assert.False(result.Passed);
        Assert.Equal(2, result.Prompts.Count);

        var france = result.Prompts[0];
        Assert.True(france.Passed);
        Assert.Equal("The capital of France is Paris.", france.ResponseText);

        var germany = result.Prompts[1];
        Assert.False(germany.Passed);
        Assert.Contains(germany.Assertions, a => !a.Passed && a.Description == "ShouldContainResponse");
        Assert.Single(result.Failures);
    }

    private static CallbackMockChatClient CapitalsClient()
    {
        return new CallbackMockChatClient(messages =>
        {
            var lastUser = messages.Last(m => m.Role == ChatRole.User).Text ?? string.Empty;
            if (lastUser.Contains("France", StringComparison.OrdinalIgnoreCase))
            {
                return "The capital of France is Paris.";
            }

            if (lastUser.Contains("Germany", StringComparison.OrdinalIgnoreCase))
            {
                return "The capital of Germany is Berlin.";
            }

            return "I don't know.";
        });
    }
}
