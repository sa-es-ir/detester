namespace Detester;

using Azure.AI.OpenAI;
using Detester.Abstraction;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

/// <summary>
/// Factory class for creating DetesterBuilder instances with configured AI clients.
/// </summary>
public static class DetesterFactory
{
    /// <summary>
    /// Creates a DetesterBuilder with OpenAI configuration.
    /// </summary>
    /// <param name="apiKey">The OpenAI API key.</param>
    /// <param name="modelName">The model name (e.g., "gpt-4", "gpt-3.5-turbo").</param>
    /// <returns>A configured DetesterBuilder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are missing.</exception>
    public static IDetesterBuilder CreateWithOpenAI(string apiKey, string modelName = "gpt-4")
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("API key cannot be null or whitespace.", nameof(apiKey));
        }

        if (string.IsNullOrWhiteSpace(modelName))
        {
            throw new ArgumentException("Model name cannot be null or whitespace.", nameof(modelName));
        }

        var openAIClient = new OpenAIClient(new ApiKeyCredential(apiKey));
        var chatClient = openAIClient.GetChatClient(modelName);

        return new DetesterBuilder(chatClient.AsIChatClient());
    }

    /// <summary>
    /// Creates a DetesterBuilder with Azure OpenAI configuration.
    /// </summary>
    /// <param name="endpoint">The Azure OpenAI endpoint URL.</param>
    /// <param name="apiKey">The Azure OpenAI API key.</param>
    /// <param name="deploymentName">The deployment name.</param>
    /// <returns>A configured DetesterBuilder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are missing.</exception>
    public static IDetesterBuilder CreateWithAzureOpenAI(string endpoint, string apiKey, string deploymentName)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException("Endpoint cannot be null or whitespace.", nameof(endpoint));
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("API key cannot be null or whitespace.", nameof(apiKey));
        }

        if (string.IsNullOrWhiteSpace(deploymentName))
        {
            throw new ArgumentException("Deployment name cannot be null or whitespace.", nameof(deploymentName));
        }

        var azureOpenAIClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
        var chatClient = azureOpenAIClient.GetChatClient(deploymentName);

        return new DetesterBuilder(chatClient.AsIChatClient());
    }

    /// <summary>
    /// Creates a DetesterBuilder with configuration from DetesterOptions.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <returns>A configured DetesterBuilder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    /// <exception cref="DetesterException">Thrown when neither OpenAI nor Azure OpenAI is configured.</exception>
    public static IDetesterBuilder Create(DetesterOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (options.IsOpenAIConfigured)
        {
            return CreateWithOpenAI(options.OpenAIApiKey!, options.ModelName ?? "gpt-4");
        }

        if (options.IsAzureOpenAIConfigured)
        {
            return CreateWithAzureOpenAI(
                options.AzureOpenAIEndpoint!,
                options.AzureOpenAIApiKey!,
                options.ModelName ?? throw new DetesterException("ModelName is required for Azure OpenAI."));
        }

        throw new DetesterException(
            "Neither OpenAI nor Azure OpenAI is properly configured. " +
            "Please set either OpenAIApiKey or both AzureOpenAIEndpoint and AzureOpenAIApiKey.");
    }

    /// <summary>
    /// Creates a DetesterBuilder with a custom IChatClient.
    /// </summary>
    /// <param name="chatClient">The custom chat client.</param>
    /// <returns>A configured DetesterBuilder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when chatClient is null.</exception>
    public static IDetesterBuilder Create(IChatClient chatClient)
    {
        if (chatClient == null)
        {
            throw new ArgumentNullException(nameof(chatClient));
        }

        return new DetesterBuilder(chatClient);
    }
}
