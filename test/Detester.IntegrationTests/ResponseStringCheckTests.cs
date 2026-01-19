namespace Detester.IntegrationTests;

using Detester;
using Detester.Abstraction;
using Microsoft.Extensions.AI;
using System.Text.Json;

public class ResponseStringCheckTests : IClassFixture<AzureOpenAIChatClientFixture>
{
    private const string GlobalInstruction =
        "You are an assistant used for automated tests. " +
        "For any prompt, respond with a single line in the following exact format: " +
        "'status: ok; tags: foo, bar, baz; id: 12345; message: hello world'. " +
        "Do not add explanations or vary the text.";

    private readonly IChatClient chatClient;

    public ResponseStringCheckTests(AzureOpenAIChatClientFixture fixture)
    {
        chatClient = fixture.ChatClient;
    }

    [Fact]
    public async Task ShouldBeEqualTo_WorksWithLiveAzureOpenAI()
    {
        var builder = new DetesterBuilder(chatClient);

        await builder
            .WithInstruction(GlobalInstruction)
            .WithPrompt("Return the test status line exactly.")
            .ShouldBeEqualTo("status: ok; tags: foo, bar, baz; id: 12345; message: hello world")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldContainAny_WorksWithLiveAzureOpenAI()
    {
        var builder = new DetesterBuilder(chatClient);

        await builder
            .WithInstruction(GlobalInstruction)
            .WithPrompt("Return the test status line exactly.")
            .ShouldContainAny("tags: foo, bar, baz", "tags: alpha, beta")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldContainAll_WorksWithLiveAzureOpenAI()
    {
        var builder = new DetesterBuilder(chatClient);

        await builder
            .WithInstruction(GlobalInstruction)
            .WithPrompt("Return the test status line exactly.")
            .ShouldContainAll("status: ok", "tags: foo, bar, baz", "id: 12345")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldMatchRegex_WorksWithLiveAzureOpenAI()
    {
        var builder = new DetesterBuilder(chatClient);

        await builder
            .WithInstruction(GlobalInstruction)
            .WithPrompt("Return the test status line exactly.")
            .ShouldMatchRegex(@"status: ok; tags: .*foo, bar, baz.*; id: [0-9]+; message: hello world")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldContainResponse_And_OrShouldContainResponse_WorkWithLiveAzureOpenAI()
    {
        var builder = new DetesterBuilder(chatClient);

        await builder
            .WithInstruction(GlobalInstruction)
            .WithPrompt("Return the test status line exactly.")
            .ShouldContainResponse("status: ok")
            .OrShouldContainResponse("status: error")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldHaveJsonOfType_ValidJson_Passes()
    {
        var builder = new DetesterBuilder(chatClient);

        const string jsonInstruction =
            "You are an assistant used for automated tests. Always respond with a single-line JSON object " +
            "with properties: status (string 'ok'), tags (array ['foo','bar','baz']), id (number 12345), " +
            "message (string 'hello world'). No extra text.";

        await builder
            .WithInstruction(jsonInstruction)
            .WithPrompt("Return the test status JSON.")
            .ShouldHaveJsonOfType<StatusResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                s => s.Status == "ok" && s.Id == 12345 && s.Tags is { Length: >= 3 })
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldHaveJsonOfType_InvalidJson_ThrowsDetesterException()
    {
        var builder = new DetesterBuilder(chatClient);

        const string nonJsonInstruction =
            "You are an assistant used for automated tests. Respond only with plain text: 'not json'.";

        await Assert.ThrowsAsync<DetesterException>(
            () => builder
                .WithInstruction(nonJsonInstruction)
                .WithPrompt("Return the non-JSON test value.")
                .ShouldHaveJsonOfType<StatusResponse>()
                .AssertAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ShouldHaveJsonOfType_ValidatorFails_ThrowsDetesterException()
    {
        var builder = new DetesterBuilder(chatClient);

        const string jsonInstruction =
            "You are an assistant used for automated tests. Always respond with a single-line JSON object " +
            "with properties: status (string 'ok'), tags (array ['foo','bar','baz']), id (number 12345), " +
            "message (string 'hello world'). No extra text.";

        await Assert.ThrowsAsync<DetesterException>(
            () => builder
                .WithInstruction(jsonInstruction)
                .WithPrompt("Return the test status JSON.")
                .ShouldHaveJsonOfType<StatusResponse>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                    s => s.Status == "error")
                .AssertAsync(TestContext.Current.CancellationToken));
    }
}
