namespace Detester.IntegrationTests;

using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using System.ClientModel;

/// <summary>
/// Shared Azure OpenAI client fixture so the chat client is created only once.
/// </summary>
public sealed class AzureOpenAIChatClientFixture : IDisposable
{
    public AzureOpenAIChatClientFixture()
    {
        var apiKey = Environment.GetEnvironmentVariable("AzureOpenAI__ApiKey");
        var endpoint = Environment.GetEnvironmentVariable("AzureOpenAI__Endpoint");
        var deploymentName = Environment.GetEnvironmentVariable("AzureOpenAI__ChatDeploymentName");

        if (string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(endpoint) ||
            string.IsNullOrWhiteSpace(deploymentName))
        {
            throw new InvalidOperationException(
                "Azure OpenAI integration test configuration is missing. " +
                "Ensure AzureOpenAI__ApiKey, AzureOpenAI__Endpoint and AzureOpenAI__ChatDeploymentName are set.");
        }

        var client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

        // Base IChatClient for Detester (no tools).
        ChatClient = client.GetChatClient(deploymentName).AsIChatClient();
    }

    /// <summary>
    /// Gets a plain chat client without tools (used by response string tests).
    /// </summary>
    public IChatClient ChatClient { get; }

    /// <summary>
    /// Creates a client configured for function-calling tests using AIFunction.
    /// </summary>
    public IChatClient CreateFunctionCallingClient()
    {
        // Wrap the base client with function invocation behavior and configured functions.
        IChatClient functionClient = ChatClient
            .AsBuilder()
            .UseFunctionInvocation(configure: client =>
            {
                client.AdditionalTools = CreateChatOptions().Tools;
            })
            .Build();

        return functionClient;
    }

    public ChatOptions CreateChatOptions()
    {
        static WeatherResult GetWeather(WeatherRequest request) =>
            new($"Weather for {request.Location} in {request.Units}.");

        var getWeatherFunction = AIFunctionFactory.Create(
            GetWeather,
            "get_weather",
            "Get the weather for a location.");

        return new ChatOptions
        {
            Tools = [getWeatherFunction],
        };
    }

    public void Dispose()
    {
        // AzureOpenAIClient is managed by the SDK; nothing explicit to dispose here.
    }

    private sealed record WeatherRequest(string Location, string Units);

    private sealed record WeatherResult(string Summary);
}
