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

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(deploymentName))
        {
            throw new InvalidOperationException(
                "Azure OpenAI integration test configuration is missing. " +
                "Ensure AzureOpenAI__ApiKey, AzureOpenAI__Endpoint and AzureOpenAI__ChatDeploymentName are set.");
        }

        var client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

        // Wrap AzureOpenAIClient in an IChatClient for Detester.
        ChatClient = client.GetChatClient(deploymentName).AsIChatClient();
    }

    public IChatClient ChatClient { get; }

    public void Dispose()
    {
        // AzureOpenAIClient is managed by the SDK; nothing explicit to dispose here.
    }
}
