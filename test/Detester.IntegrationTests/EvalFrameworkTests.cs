namespace Detester.IntegrationTests;

using Detester;
using Detester.Abstraction;
using Microsoft.Extensions.AI;

/// <summary>
/// Integration tests for the eval-framework features (per-prompt scoping, EvaluateAsync,
/// semantic similarity, LLM-as-judge, dataset evaluation) against a real Azure OpenAI endpoint.
/// </summary>
public class EvalFrameworkTests : IClassFixture<AzureOpenAIChatClientFixture>
{
    private const string DeterministicInstruction =
        "You are an assistant used for automated tests. Answer concisely and factually.";

    private readonly AzureOpenAIChatClientFixture fixture;
    private readonly IChatClient chatClient;

    public EvalFrameworkTests(AzureOpenAIChatClientFixture fixture)
    {
        this.fixture = fixture;
        chatClient = fixture.ChatClient;
    }

    [Fact]
    public async Task PerPromptScoping_EachAssertionBindsToItsOwnResponse()
    {
        await new DetesterBuilder(chatClient)
            .WithInstruction(DeterministicInstruction)
            .WithPrompt("What is the capital of France? Answer with just the city name.")
            .ShouldContainResponse("Paris")
            .WithPrompt("What is the capital of Japan? Answer with just the city name.")
            .ShouldContainResponse("Tokyo")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EvaluateAsync_ReturnsStructuredResultWithoutThrowing()
    {
        var result = await new DetesterBuilder(chatClient)
            .WithInstruction(DeterministicInstruction)
            .WithPrompt("What is 2 + 2? Answer with just the number.")
            .ShouldContainResponse("4")
            .EvaluateAsync(TestContext.Current.CancellationToken);

        Assert.True(result.Passed);
        Assert.Single(result.Prompts);
        Assert.NotEmpty(result.Prompts[0].ResponseText);
    }

    [Fact]
    public async Task ShouldSatisfy_UsesLlmJudge()
    {
        await new DetesterBuilder(chatClient)
            .WithInstruction(DeterministicInstruction)
            .WithPrompt("Write a one-sentence positive product review for a coffee mug.")
            .ShouldSatisfy("the response expresses a clearly positive sentiment about a product")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldSatisfy_FailsWhenCriteriaNotMet()
    {
        await Assert.ThrowsAsync<DetesterException>(() =>
            new DetesterBuilder(chatClient)
                .WithInstruction(DeterministicInstruction)
                .WithPrompt("Say only the word 'hello'.")
                .ShouldSatisfy("the response contains a detailed financial analysis with numbers")
                .AssertAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ShouldBeSemanticallySimilarTo_PassesForParaphrase()
    {
        var generator = fixture.CreateEmbeddingGenerator();
        Assert.SkipWhen(
            generator is null,
            "AzureOpenAI__EmbeddingDeploymentName not configured; skipping semantic similarity test.");

        await new DetesterBuilder(chatClient)
            .WithEmbeddingGenerator(generator!)
            .WithInstruction(DeterministicInstruction)
            .WithPrompt("In one short sentence, what is the capital of France?")
            .ShouldBeSemanticallySimilarTo("The capital of France is Paris.", 0.6)
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldBeSemanticallySimilarTo_FailsForUnrelatedText()
    {
        var generator = fixture.CreateEmbeddingGenerator();
        Assert.SkipWhen(
            generator is null,
            "AzureOpenAI__EmbeddingDeploymentName not configured; skipping semantic similarity test.");

        await Assert.ThrowsAsync<DetesterException>(() =>
            new DetesterBuilder(chatClient)
                .WithEmbeddingGenerator(generator!)
                .WithInstruction(DeterministicInstruction)
                .WithPrompt("In one short sentence, what is the capital of France?")
                .ShouldBeSemanticallySimilarTo("Photosynthesis converts sunlight into chemical energy in plants.", 0.85)
                .AssertAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DatasetEvaluation_RunsAllCasesAndAggregates()
    {
        var cases = new[]
        {
            new DatasetCase { Input = "Capital of France? One word.", Expected = "Paris", Name = "fr" },
            new DatasetCase { Input = "Capital of Italy? One word.", Expected = "Rome", Name = "it" },
            new DatasetCase { Input = "Capital of Spain? One word.", Expected = "Madrid", Name = "es" },
        };

        var result = await DetesterDataset.EvaluateAsync(
            chatClient,
            cases,
            (builder, c) => builder
                .WithInstruction(DeterministicInstruction)
                .WithPrompt(c.Input)
                .ShouldContainResponse(c.Expected!),
            TestContext.Current.CancellationToken);

        Assert.Equal(3, result.Cases.Count);
        Assert.True(result.PassRate >= 0.66, $"Expected most cases to pass, got {result.PassRate:P0}");
    }
}
