<p align="center"><img src="detester_logo.png" width="300"></p>

<div align="center">

[![NuGet](https://img.shields.io/nuget/v/Detester.svg)](https://www.nuget.org/packages/Detester/)
[![GitHub stars](https://img.shields.io/github/stars/sa-es-ir/Detester.svg)](https://github.com/sa-es-ir/Detester/stargazers)
</div>

# Detester

AI Deterministic Tester - A testing framework for building deterministic and reliable tests for AI applications.

## Overview

Detester is a .NET library that enables you to write deterministic tests for AI-powered applications. It provides a fluent builder API for testing AI responses, ensuring consistency and reliability in your AI integrations.

## Features

- **Fluent Builder API**: Chain multiple prompts and assertions in a readable, intuitive way
- **Multiple AI Provider Support**: Works with OpenAI, Azure OpenAI, and custom `IChatClient` implementations
- **Model Instructions**: Set system messages to guide model behavior and responses
- **Response Validation**: Assert that AI responses contain expected keywords or text
- **Function/Tool Call Verification**: Verify that AI models call the correct functions with expected parameters
- **JSON Response Validation**: Deserialize and validate JSON responses from AI models with type-safe validation
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

### Setting Model Instructions

Set custom instructions (system messages) to guide the model's behavior:

```csharp
await builder
    .WithInstruction("You are a helpful assistant that provides concise answers.")
    .WithPrompt("What is machine learning?")
    .ShouldContainResponse("algorithm")
    .AssertAsync();
```

Instructions are sent as system messages before any prompts, allowing you to control the model's tone, style, and behavior throughout the conversation:

```csharp
await builder
    .WithInstruction("You are a Python expert. Always provide code examples.")
    .WithPrompt("How do I read a file in Python?")
    .ShouldContainResponse("open(")
    .ShouldContainResponse("read(")
    .AssertAsync();
```

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

### OR Assertions

Use `OrShouldContainResponse` to create flexible response validation where at least one of the alternatives must match:

```csharp
await builder
    .WithPrompt("What is the capital of France?")
    .ShouldContainResponse("capital")
    .OrShouldContainResponse("city")
    .OrShouldContainResponse("Paris")
    .AssertAsync();
```

In this example, the test passes if the response contains "capital" OR "city" OR "Paris". You can chain multiple OR conditions, and the test will pass if any one of them is found in the response.

#### Combining AND and OR Assertions

You can mix `ShouldContainResponse` (AND) with `OrShouldContainResponse` (OR) for complex validation:

```csharp
await builder
    .WithPrompt("Explain machine learning")
    .ShouldContainResponse("algorithm")  // Must contain "algorithm"
    .ShouldContainResponse("data")       // AND must contain "data"
    .OrShouldContainResponse("train")    // AND must contain "train" OR "data"
    .AssertAsync();
```

Note: `OrShouldContainResponse` creates an OR group with the immediately preceding assertion. Each subsequent `OrShouldContainResponse` adds another alternative to that OR group.

### Function/Tool Call Verification

Detester supports verifying that AI models call the correct functions/tools with expected parameters. This is useful for testing AI applications that use function calling capabilities.

#### Basic Function Call Verification

Verify that a specific function is called:

```csharp
await builder
    .WithPrompt("What's the weather in Paris?")
    .ShouldCallFunction("get_weather")
    .AssertAsync();
```

#### Verify Function Parameters

Check that functions are called with the correct parameters:

```csharp
await builder
    .WithPrompt("What's the weather in Paris in celsius?")
    .ShouldCallFunctionWithParameters("get_weather", 
        new Dictionary<string, object?> 
        { 
            { "location", "Paris" },
            { "units", "celsius" }
        })
    .AssertAsync();
```

#### Multiple Function Calls

Verify multiple function calls in a single response:

```csharp
await builder
    .WithPrompt("Compare the weather in Paris and London")
    .ShouldCallFunction("get_weather")
    .ShouldCallFunction("get_weather")
    .AssertAsync();
```

#### Combined Verification

Combine function call verification with text response assertions:

```csharp
await builder
    .WithPrompt("What's the capital of France?")
    .ShouldCallFunction("get_capital")
    .ShouldContainResponse("Paris")
    .AssertAsync();
```

For more detailed information and examples, see the [Function Calling Guide](docs/function-calling-guide.md).

### JSON Response Validation

Detester supports validating JSON responses from AI models by deserializing them to C# types and optionally validating the deserialized objects. This is useful for testing structured outputs from language models.

#### Basic JSON Validation

Verify that the response can be deserialized to a specific type:

```csharp
public class User
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int Age { get; set; }
    public DateTime JoinDate { get; set; }
}

await builder
    .WithPrompt("Who is the last user joined?")
    .ShouldHaveJsonOfType<User>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
    .AssertAsync();
```

#### JSON Validation with Custom Validation

Add custom validation logic to verify the deserialized object:

```csharp
await builder
    .WithPrompt("Who is the last user joined?")
    .ShouldHaveJsonOfType<User>(
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
        user => user.Age > 30 && user.FirstName!.Contains("Jo"))
    .AssertAsync();
```

#### Complex JSON Validation

Combine multiple validations:

```csharp
await builder
    .WithPrompt("Get user details")
    .ShouldContainResponse("Joe")  // Text assertion
    .ShouldHaveJsonOfType<User>(
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
        user => user.Age > 18)  // JSON validation
    .ShouldHaveJsonOfType<User>(
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
        user => user.LastName == "Doe")  // Additional JSON validation
    .AssertAsync();
```

**Note:** 
- The JSON validation uses `System.Text.Json` for deserialization
- Deserialization exceptions are caught and wrapped in `DetesterException` with helpful error messages
- If validation fails, the test throws `DetesterException` with details about what went wrong
- For case-insensitive property name matching, use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`

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

- `WithInstruction(instruction)`: Set the instruction (system message) for the AI model
- `WithPrompt(prompt)`: Add a single prompt
- `WithPrompts(params prompts)`: Add multiple prompts
- `ShouldContainResponse(expectedText)`: Assert response contains text (case-insensitive, AND condition)
- `OrShouldContainResponse(expectedText)`: Assert response contains alternative text (case-insensitive, OR condition)
- `ShouldCallFunction(functionName)`: Assert that a specific function/tool was called
- `ShouldCallFunctionWithParameters(functionName, parameters)`: Assert that a function was called with specific parameters
- `ShouldHaveJsonOfType<T>(options, validator)`: Assert that response contains valid JSON deserializable to type T, with optional validation
- `AssertAsync(cancellationToken)`: Assert the test by executing prompts and validating responses

## Error Handling

Detester throws `DetesterException` when:
- No prompts are provided before execution
- Expected text is not found in the response
- None of the OR alternatives are found in the response
- Configuration is invalid

Detester throws `InvalidOperationException` when:
- `OrShouldContainResponse` is called without a prior assertion

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
