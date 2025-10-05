# Detester

AI Deterministic Tester - A testing framework for building deterministic and reliable tests for AI applications.

## Overview

Detester is a .NET library that enables you to write deterministic tests for AI-powered applications. It provides a fluent builder API for testing AI responses, ensuring consistency and reliability in your AI integrations.

## Features

- **Fluent Builder API**: Chain multiple prompts and assertions in a readable, intuitive way
- **Multiple AI Provider Support**: Works with OpenAI, Azure OpenAI, and custom `IChatClient` implementations
- **Response Validation**: Assert that AI responses contain expected keywords or text
- **Method Chaining**: Combine multiple prompts and assertions in a single test flow
- **Extensible**: Build on Microsoft.Extensions.AI abstractions for maximum flexibility

## Installation

```bash
dotnet add package Detester
```

## Quick Start

### Using OpenAI

```csharp
using Detester;

// Create a builder with OpenAI
var builder = DetesterFactory.CreateWithOpenAI(
    apiKey: "your-openai-api-key",
    modelName: "gpt-4");

// Execute a test
await builder
    .WithPrompt("What is the capital of France?")
    .ShouldContainResponse("Paris")
    .AssertAsync();
```

### Using Azure OpenAI

```csharp
using Detester;

// Create a builder with Azure OpenAI
var builder = DetesterFactory.CreateWithAzureOpenAI(
    endpoint: "https://your-resource.openai.azure.com",
    apiKey: "your-azure-api-key",
    deploymentName: "gpt-4");

// Execute a test
await builder
    .WithPrompt("Explain quantum computing in simple terms")
    .ShouldContainResponse("quantum")
    .AssertAsync();
```

### Using Configuration Options

```csharp
using Detester.Abstraction;

var options = new DetesterOptions
{
    OpenAIApiKey = "your-openai-api-key",
    ModelName = "gpt-4"
};

var builder = DetesterFactory.Create(options);

await builder
    .WithPrompt("Tell me a joke")
    .ShouldContainResponse("joke")
    .AssertAsync();
```

## Advanced Usage

### Multiple Prompts

Test conversational flows by chaining multiple prompts:

```csharp
await builder
    .WithPrompt("Hello, I need help with coding")
    .WithPrompt("Can you explain what a variable is?")
    .ShouldContainResponse("variable")
    .AssertAsync();
```

### Multiple Assertions

Add multiple response checks:

```csharp
await builder
    .WithPrompt("Write a haiku about programming")
    .ShouldContainResponse("code")
    .ShouldContainResponse("lines")
    .AssertAsync();
```

### Using Custom IChatClient

Integrate with your own chat client implementation:

```csharp
using Microsoft.Extensions.AI;

IChatClient customClient = // your custom implementation
var builder = DetesterFactory.Create(customClient);

await builder
    .WithPrompt("Test prompt")
    .ShouldContainResponse("expected text")
    .AssertAsync();
```

### Batch Prompts

Add multiple prompts at once:

```csharp
await builder
    .WithPrompts(
        "What is machine learning?",
        "How does it differ from traditional programming?",
        "Give me a practical example")
    .ShouldContainResponse("algorithm")
    .ShouldContainResponse("data")
    .AssertAsync();
```

## Testing Example with xUnit

```csharp
public class AITests
{
    [Fact]
    public async Task TestAIResponse()
    {
        // Arrange
        var builder = DetesterFactory.CreateWithOpenAI(
            Environment.GetEnvironmentVariable("OPENAI_API_KEY")!,
            "gpt-4");

        // Act & Assert
        await builder
            .WithPrompt("What is 2+2?")
            .ShouldContainResponse("4")
            .AssertAsync();
    }
}
```

## Configuration

### OpenAI Configuration

Set the following environment variables or pass them directly:
- `OPENAI_API_KEY`: Your OpenAI API key

### Azure OpenAI Configuration

Set the following configuration:
- `AZURE_OPENAI_ENDPOINT`: Your Azure OpenAI endpoint URL
- `AZURE_OPENAI_API_KEY`: Your Azure OpenAI API key
- `MODEL_NAME`: Your deployment name

## API Reference

### DetesterFactory

- `CreateWithOpenAI(apiKey, modelName)`: Create a builder for OpenAI
- `CreateWithAzureOpenAI(endpoint, apiKey, deploymentName)`: Create a builder for Azure OpenAI
- `Create(options)`: Create a builder from configuration options
- `Create(chatClient)`: Create a builder with a custom IChatClient

### IDetesterBuilder

- `WithPrompt(prompt)`: Add a single prompt
- `WithPrompts(params prompts)`: Add multiple prompts
- `ShouldContainResponse(expectedText)`: Assert response contains text (case-insensitive)
- `AssertAsync(cancellationToken)`: Assert the test by executing prompts and validating responses

## Error Handling

Detester throws `DetesterException` when:
- No prompts are provided before execution
- Expected text is not found in the response
- Configuration is invalid

Example:

```csharp
try
{
    await builder
        .WithPrompt("What is AI?")
        .ShouldContainResponse("impossible text that won't appear")
        .AssertAsync();
}
catch (DetesterException ex)
{
    Console.WriteLine($"Test failed: {ex.Message}");
}
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License.

## Acknowledgments

Built on top of [Microsoft.Extensions.AI](https://www.nuget.org/packages/Microsoft.Extensions.AI) for seamless integration with AI services.
