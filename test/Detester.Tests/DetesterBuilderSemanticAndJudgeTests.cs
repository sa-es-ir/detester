// Copyright (c) Detester. All rights reserved.

namespace Detester.Tests;

using Detester.Abstraction;
using Microsoft.Extensions.AI;

/// <summary>
/// Tests for semantic similarity, LLM-as-judge, and negative function-call assertions.
/// </summary>
public class DetesterBuilderSemanticAndJudgeTests
{
    private static readonly float[] VectorA = [1f, 0f, 0f];
    private static readonly float[] VectorAClose = [0.99f, 0.14f, 0f];
    private static readonly float[] VectorOrthogonal = [0f, 1f, 0f];

    [Fact]
    public async Task ShouldBeSemanticallySimilarTo_WhenVectorsAligned_Passes()
    {
        var mockClient = new MockChatClient { ResponseText = "a paraphrase of the answer" };
        var generator = new FakeEmbeddingGenerator(_ => VectorA);
        var builder = new DetesterBuilder(mockClient);

        await builder
            .WithEmbeddingGenerator(generator)
            .WithPrompt("Question")
            .ShouldBeSemanticallySimilarTo("the expected answer", 0.8)
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldBeSemanticallySimilarTo_WhenOrthogonal_Fails()
    {
        var mockClient = new MockChatClient { ResponseText = "totally unrelated text" };
        var generator = new FakeEmbeddingGenerator(text =>
            text.Contains("expected", StringComparison.OrdinalIgnoreCase) ? VectorOrthogonal : VectorA);
        var builder = new DetesterBuilder(mockClient);

        var ex = await Assert.ThrowsAsync<DetesterException>(() =>
            builder
                .WithEmbeddingGenerator(generator)
                .WithPrompt("Question")
                .ShouldBeSemanticallySimilarTo("the expected answer", 0.8)
                .AssertAsync(TestContext.Current.CancellationToken));

        Assert.Contains("semantically similar", ex.Message);
    }

    [Fact]
    public async Task ShouldBeSemanticallySimilarTo_NearlyAligned_PassesWithLowerThreshold()
    {
        var mockClient = new MockChatClient { ResponseText = "response" };
        var generator = new FakeEmbeddingGenerator(text =>
            text.Contains("expected", StringComparison.OrdinalIgnoreCase) ? VectorAClose : VectorA);
        var builder = new DetesterBuilder(mockClient);

        await builder
            .WithEmbeddingGenerator(generator)
            .WithPrompt("Question")
            .ShouldBeSemanticallySimilarTo("the expected answer", 0.9)
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldBeSemanticallySimilarTo_WithoutGenerator_ThrowsInvalidOperation()
    {
        var mockClient = new MockChatClient { ResponseText = "response" };
        var builder = new DetesterBuilder(mockClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            builder
                .WithPrompt("Question")
                .ShouldBeSemanticallySimilarTo("expected")
                .AssertAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldBeSemanticallySimilarTo_WithInvalidScore_ThrowsArgumentException()
    {
        var builder = new DetesterBuilder(new MockChatClient());

        Assert.Throws<ArgumentException>(() => builder.ShouldBeSemanticallySimilarTo("x", 2.0));
    }

    [Fact]
    public async Task ShouldSatisfy_WhenJudgeReturnsPass_Passes()
    {
        var mainClient = new MockChatClient { ResponseText = "The product is great and I love it." };
        var judge = new CallbackMockChatClient(_ => "PASS");
        var builder = new DetesterBuilder(mainClient);

        await builder
            .WithJudge(judge)
            .WithPrompt("Write a positive review")
            .ShouldSatisfy("the response expresses a positive sentiment")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldSatisfy_WhenJudgeReturnsFail_Throws()
    {
        var mainClient = new MockChatClient { ResponseText = "This is terrible." };
        var judge = new CallbackMockChatClient(_ => "FAIL: sentiment is negative");
        var builder = new DetesterBuilder(mainClient);

        var ex = await Assert.ThrowsAsync<DetesterException>(() =>
            builder
                .WithJudge(judge)
                .WithPrompt("Write a positive review")
                .ShouldSatisfy("the response expresses a positive sentiment")
                .AssertAsync(TestContext.Current.CancellationToken));

        Assert.Contains("did not satisfy the criteria", ex.Message);
        Assert.Contains("sentiment is negative", ex.Message);
    }

    [Fact]
    public async Task ShouldSatisfy_JudgeReceivesCriteriaAndResponse()
    {
        var mainClient = new MockChatClient { ResponseText = "blue elephant" };
        string? capturedUserPrompt = null;
        var judge = new CallbackMockChatClient(messages =>
        {
            capturedUserPrompt = messages.Last(m => m.Role == ChatRole.User).Text;
            return "PASS";
        });
        var builder = new DetesterBuilder(mainClient);

        await builder
            .WithJudge(judge)
            .WithPrompt("Say something")
            .ShouldSatisfy("mentions an animal")
            .AssertAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(capturedUserPrompt);
        Assert.Contains("mentions an animal", capturedUserPrompt);
        Assert.Contains("blue elephant", capturedUserPrompt);
    }

    [Fact]
    public async Task ShouldNotCallFunction_WhenNotCalled_Passes()
    {
        var mockClient = new MockChatClient
        {
            ResponseText = "No tools needed",
            FunctionCallsToReturn = [],
        };
        var builder = new DetesterBuilder(mockClient);

        await builder
            .WithPrompt("Just answer in text")
            .ShouldNotCallFunction("delete_everything")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldNotCallFunction_WhenCalled_Throws()
    {
        var mockClient = new MockChatClient
        {
            ResponseText = string.Empty,
            FunctionCallsToReturn =
            [
                new FunctionCallContent("call-1", "delete_everything", new Dictionary<string, object?>())
            ],
        };
        var builder = new DetesterBuilder(mockClient);

        var ex = await Assert.ThrowsAsync<DetesterException>(() =>
            builder
                .WithPrompt("Do something")
                .ShouldNotCallFunction("delete_everything")
                .AssertAsync(TestContext.Current.CancellationToken));

        Assert.Contains("NOT to be called", ex.Message);
    }

    [Fact]
    public void ShouldNotCallFunction_WithNullName_ThrowsArgumentException()
    {
        var builder = new DetesterBuilder(new MockChatClient());

        Assert.Throws<ArgumentException>(() => builder.ShouldNotCallFunction(null!));
    }
}
