namespace Detester.IntegrationTests;

using Detester;
using Microsoft.Extensions.AI;

public class FunctionCallTests : IClassFixture<AzureOpenAIChatClientFixture>
{
    private readonly IChatClient functionClient;
    private readonly ChatOptions chatOptions;

    public FunctionCallTests(AzureOpenAIChatClientFixture fixture)
    {
        functionClient = fixture.CreateFunctionCallingClient();
        chatOptions = fixture.CreateChatOptions();
    }

    [Fact(Skip = "Function calling tests are not yet implemented.")]
    public async Task ShouldCallFunction_WithExpectedFunction_Succeeds()
    {
        var builder = new DetesterBuilder(functionClient);

        const string instruction =
            "You are an assistant used for automated tests. " +
            "Use the available tools instead of answering directly. " +
            "When asked about the weather, you MUST call the get_weather tool.";

        await builder
            .WithInstruction(instruction)
            .WithPrompt("What is the weather in Paris in celsius?")
            .ShouldCallFunction("get_weather")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact(Skip = "Function calling tests are not yet implemented.")]
    public async Task ShouldCallFunctionWithParameters_WithExpectedArguments_Succeeds()
    {
        var builder = new DetesterBuilder(functionClient);

        const string instruction =
            "You are an assistant used for automated tests. " +
            "Use the available tools instead of answering directly. " +
            "When asked about the weather in Paris in celsius, " +
            "you MUST call get_weather with location 'Paris' and units 'celsius'.";

        var expectedParameters = new Dictionary<string, object?>
        {
            { "location", "Paris" },
            { "units", "celsius" },
        };

        await builder
            .WithInstruction(instruction)
            .WithPrompt("What is the weather in Paris in celsius?")
            .ShouldCallFunctionWithParameters("get_weather", expectedParameters)
            .AssertAsync(TestContext.Current.CancellationToken);
    }
}