namespace Detester.IntegrationTests;

using Detester;
using Detester.Abstraction;
using Microsoft.Extensions.AI;

/// <summary>
/// Integration tests for reliability evaluation, latency assertions,
/// and token usage assertions — tested against a real Azure OpenAI endpoint.
/// </summary>
public class ReliabilityAndPerformanceTests : IClassFixture<AzureOpenAIChatClientFixture>
{
    private const string DeterministicInstruction =
        "You are an assistant used for automated tests. " +
        "For any prompt, respond with a single line in the following exact format: " +
        "'status: ok; tags: foo, bar, baz; id: 12345; message: hello world'. " +
        "Do not add explanations or vary the text.";

    private readonly IChatClient chatClient;

    public ReliabilityAndPerformanceTests(AzureOpenAIChatClientFixture fixture)
    {
        chatClient = fixture.ChatClient;
    }

    [Fact]
    public async Task ShouldRespondWithin_ReasonableLimit_Passes()
    {
        await new DetesterBuilder(chatClient)
            .WithInstruction(DeterministicInstruction)
            .WithPrompt("Return the test status line exactly.")
            .ShouldContainResponse("status: ok")
            .ShouldRespondWithin(TimeSpan.FromSeconds(30))
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldRespondWithin_ImpossiblyTightLimit_ThrowsDetesterException()
    {
        await Assert.ThrowsAsync<DetesterException>(() =>
            new DetesterBuilder(chatClient)
                .WithInstruction(DeterministicInstruction)
                .WithPrompt("Return the test status line exactly.")
                .ShouldRespondWithin(TimeSpan.FromMilliseconds(1))
                .AssertAsync(TestContext.Current.CancellationToken));
    }

    // -------------------------------------------------------------------------
    // ShouldUseTokensUnder
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ShouldUseTokensUnder_GenerousLimit_Passes()
    {
        await new DetesterBuilder(chatClient)
            .WithInstruction(DeterministicInstruction)
            .WithPrompt("Return the test status line exactly.")
            .ShouldContainResponse("status: ok")
            .ShouldUseTokensUnder(2000)
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldUseTokensUnder_TightLimit_ThrowsDetesterException()
    {
        await Assert.ThrowsAsync<DetesterException>(() =>
            new DetesterBuilder(chatClient)
                .WithInstruction(DeterministicInstruction)
                .WithPrompt("Return the test status line exactly.")
                .ShouldUseTokensUnder(1)
                .AssertAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ShouldUseCompletionTokensUnder_GenerousLimit_Passes()
    {
        await new DetesterBuilder(chatClient)
            .WithInstruction(DeterministicInstruction)
            .WithPrompt("Return the test status line exactly.")
            .ShouldContainResponse("status: ok")
            .ShouldUseCompletionTokensUnder(500)
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldUseCompletionTokensUnder_TightLimit_ThrowsDetesterException()
    {
        await Assert.ThrowsAsync<DetesterException>(() =>
            new DetesterBuilder(chatClient)
                .WithInstruction(DeterministicInstruction)
                .WithPrompt("Return the test status line exactly.")
                .ShouldUseCompletionTokensUnder(1)
                .AssertAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AssertReliablyAsync_DeterministicResponse_AllRunsPass()
    {
        var result = await new DetesterBuilder(chatClient)
            .WithInstruction(DeterministicInstruction)
            .WithPrompt("Return the test status line exactly.")
            .ShouldContainResponse("status: ok")
            .AssertReliablyAsync(runs: 3, requiredPassRate: 1.0, TestContext.Current.CancellationToken);

        Assert.Equal(3, result.TotalRuns);
        Assert.Equal(3, result.PassCount);
        Assert.Equal(0, result.FailCount);
        Assert.Equal(1.0, result.PassRate);
        Assert.Empty(result.Failures);
    }

    [Fact]
    public async Task AssertReliablyAsync_ImpossibleAssertion_FailsWithDetails()
    {
        var ex = await Assert.ThrowsAsync<DetesterException>(() =>
            new DetesterBuilder(chatClient)
                .WithInstruction(DeterministicInstruction)
                .WithPrompt("Return the test status line exactly.")
                .ShouldContainResponse("this text will never appear in any response xyz987")
                .AssertReliablyAsync(runs: 2, requiredPassRate: 0.5, TestContext.Current.CancellationToken));

        Assert.Contains("0/2 runs passed", ex.Message);
        Assert.Contains("Run 1:", ex.Message);
        Assert.Contains("Run 2:", ex.Message);
    }

    [Fact]
    public async Task AssertReliablyAsync_WithLatencyAndTokenAssertions_Passes()
    {
        var result = await new DetesterBuilder(chatClient)
            .WithInstruction(DeterministicInstruction)
            .WithPrompt("Return the test status line exactly.")
            .ShouldContainResponse("status: ok")
            .ShouldRespondWithin(TimeSpan.FromSeconds(30))
            .ShouldUseTokensUnder(2000)
            .ShouldUseCompletionTokensUnder(500)
            .AssertReliablyAsync(runs: 2, requiredPassRate: 1.0, TestContext.Current.CancellationToken);

        Assert.Equal(2, result.PassCount);
        Assert.Equal(1.0, result.PassRate);
    }

    [Fact]
    public async Task AssertReliablyAsync_ReturnsResultEvenAtExactThreshold()
    {
        // With a deterministic mock instruction and 100% required pass rate,
        // verifies the result object is returned correctly when threshold is exactly met.
        var result = await new DetesterBuilder(chatClient)
            .WithInstruction(DeterministicInstruction)
            .WithPrompt("Return the test status line exactly.")
            .ShouldContainResponse("status: ok")
            .AssertReliablyAsync(runs: 2, requiredPassRate: 1.0, TestContext.Current.CancellationToken);

        Assert.Equal(result.PassCount, result.TotalRuns);
        Assert.Equal(1.0, result.PassRate);
    }
}
