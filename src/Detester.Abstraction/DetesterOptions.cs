// Copyright (c) Detester. All rights reserved.

namespace Detester.Abstraction;

/// <summary>
/// Configuration options for OpenAI or Azure OpenAI.
/// </summary>
public class DetesterOptions
{
    /// <summary>
    /// Gets or sets the OpenAI API key.
    /// </summary>
    public string? OpenAIApiKey { get; set; }

    /// <summary>
    /// Gets or sets the Azure OpenAI endpoint.
    /// </summary>
    public string? AzureOpenAIEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the Azure OpenAI API key.
    /// </summary>
    public string? AzureOpenAIApiKey { get; set; }

    /// <summary>
    /// Gets or sets the model deployment name (for Azure OpenAI) or model name (for OpenAI).
    /// </summary>
    public string? ModelName { get; set; }

    /// <summary>
    /// Gets a value indicating whether OpenAI configuration is set.
    /// </summary>
    public bool IsOpenAIConfigured => !string.IsNullOrWhiteSpace(this.OpenAIApiKey);

    /// <summary>
    /// Gets a value indicating whether Azure OpenAI configuration is set.
    /// </summary>
    public bool IsAzureOpenAIConfigured =>
        !string.IsNullOrWhiteSpace(this.AzureOpenAIEndpoint) &&
        !string.IsNullOrWhiteSpace(this.AzureOpenAIApiKey);
}
