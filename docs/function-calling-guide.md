# Function/Tool Call Verification Guide

This guide provides comprehensive documentation on how to use Detester to verify function and tool calls made by AI models in response to prompts.

## Table of Contents

- [Overview](#overview)
- [Basic Concepts](#basic-concepts)
- [Getting Started](#getting-started)
- [API Reference](#api-reference)
- [Examples](#examples)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

## Overview

Function calling (also known as tool calling) is a powerful feature of modern AI models that allows them to interact with external functions to retrieve data or perform actions. Detester provides a fluent API to verify that your AI model calls the correct functions with the expected parameters.

### What Can You Verify?

- ✅ That a specific function was called
- ✅ Function parameters and their values
- ✅ Multiple function calls in a single response
- ✅ Combination of function calls and text responses

## Basic Concepts

### Function Calls in Microsoft.Extensions.AI

When an AI model decides to call a function, the response contains `FunctionCallContent` objects within the `ChatMessage.Contents` collection. Each function call includes:

- **Name**: The function name
- **CallId**: A unique identifier for the call
- **Arguments**: A dictionary of parameter names and values

### How Detester Works

Detester intercepts the AI model's response and verifies that:
1. The expected functions were called
2. The parameters match your expectations (if specified)
3. All assertions pass before the test completes

## Getting Started

### Prerequisites

- .NET 8.0 or higher
- Detester NuGet package installed
- An AI model configured with function/tool capabilities

### Basic Setup

```csharp
using Detester;
using Detester.Abstraction;
using Microsoft.Extensions.AI;

// Create your chat client (e.g., using OpenAI)
// var openAIClient = new OpenAIClient("your-api-key");
// var chatClient = openAIClient.GetChatClient("gpt-4").AsIChatClient();

// Create a builder with the chat client
var builder = DetesterFactory.Create(chatClient);
```

## API Reference

### ShouldCallFunction

Verifies that a function with the specified name was called.

```csharp
IDetesterBuilder ShouldCallFunction(string functionName)
```

**Parameters:**
- `functionName` (string): The name of the function to verify

**Returns:**
- The builder instance for method chaining

**Throws:**
- `ArgumentException`: If functionName is null or whitespace
- `DetesterException`: If the function was not called during assertion

**Example:**
```csharp
await builder
    .WithPrompt("What's the weather in Paris?")
    .ShouldCallFunction("get_weather")
    .AssertAsync();
```

### ShouldCallFunctionWithParameters

Verifies that a function was called with specific parameters.

```csharp
IDetesterBuilder ShouldCallFunctionWithParameters(
    string functionName, 
    IDictionary<string, object?> expectedParameters)
```

**Parameters:**
- `functionName` (string): The name of the function to verify
- `expectedParameters` (IDictionary<string, object?>): Expected parameter names and values

**Returns:**
- The builder instance for method chaining

**Throws:**
- `ArgumentException`: If functionName is null or whitespace
- `ArgumentNullException`: If expectedParameters is null
- `DetesterException`: If the function was not called with matching parameters

**Example:**
```csharp
await builder
    .WithPrompt("What's the weather in Paris?")
    .ShouldCallFunctionWithParameters("get_weather", 
        new Dictionary<string, object?> 
        { 
            { "location", "Paris" },
            { "units", "celsius" }
        })
    .AssertAsync();
```

## Examples

### Example 1: Basic Function Call Verification

Verify that a simple function is called:

```csharp
[Fact]
public async Task TestWeatherFunctionCall()
{
    // Setup your chat client
    var chatClient = // your IChatClient implementation
    var builder = DetesterFactory.Create(chatClient);
    
    await builder
        .WithPrompt("What's the weather in Paris?")
        .ShouldCallFunction("get_weather")
        .AssertAsync();
}
```

### Example 2: Verify Function Parameters

Ensure the function is called with specific parameters:

```csharp
[Fact]
public async Task TestWeatherFunctionWithLocation()
{
    var chatClient = // your IChatClient implementation
    var builder = DetesterFactory.Create(chatClient);
    
    await builder
        .WithPrompt("What's the weather in Paris in celsius?")
        .ShouldCallFunctionWithParameters("get_weather", 
            new Dictionary<string, object?> 
            { 
                { "location", "Paris" },
                { "units", "celsius" }
            })
        .AssertAsync();
}
```

### Example 3: Multiple Function Calls

Verify multiple functions are called:

```csharp
[Fact]
public async Task TestMultipleCityWeatherCalls()
{
    var chatClient = // your IChatClient implementation
    var builder = DetesterFactory.Create(chatClient);
    
    await builder
        .WithPrompt("Compare the weather in Paris and London")
        .ShouldCallFunction("get_weather")
        .ShouldCallFunction("get_weather")
        .AssertAsync();
}
```

### Example 4: Different Functions

Verify different functions are called:

```csharp
[Fact]
public async Task TestWeatherAndTimezoneFunctions()
{
    var chatClient = // your IChatClient implementation
    var builder = DetesterFactory.Create(chatClient);
    
    await builder
        .WithPrompt("What's the weather and timezone in Tokyo?")
        .ShouldCallFunction("get_weather")
        .ShouldCallFunction("get_timezone")
        .AssertAsync();
}
```

### Example 5: Combined Function and Text Verification

Verify both function calls and response content:

```csharp
[Fact]
public async Task TestFunctionCallAndResponse()
{
    var chatClient = // your IChatClient implementation
    var builder = DetesterFactory.Create(chatClient);
    
    await builder
        .WithPrompt("What's the capital of France?")
        .ShouldCallFunction("get_capital")
        .ShouldContainResponse("Paris")
        .AssertAsync();
}
```

### Example 6: With Instructions

Use system instructions to guide function calling:

```csharp
[Fact]
public async Task TestWithInstructions()
{
    var chatClient = // your IChatClient implementation
    var builder = DetesterFactory.Create(chatClient);
    
    await builder
        .WithInstruction("Always use the get_weather function to retrieve weather data.")
        .WithPrompt("Is it sunny in Paris?")
        .ShouldCallFunction("get_weather")
        .ShouldContainResponse("weather")
        .AssertAsync();
}
```

### Example 7: Multiple Prompts

Test conversational flows with function calls:

```csharp
[Fact]
public async Task TestConversationalFunctionCalls()
{
    var chatClient = // your IChatClient implementation
    var builder = DetesterFactory.Create(chatClient);
    
    await builder
        .WithPrompt("What's the weather in Paris?")
        .WithPrompt("And what about London?")
        .ShouldCallFunction("get_weather")
        .ShouldCallFunction("get_weather")
        .AssertAsync();
}
```

### Example 8: Numeric Parameters

Verify functions with numeric parameters:

```csharp
[Fact]
public async Task TestNumericParameters()
{
    var chatClient = // your IChatClient implementation
    var builder = DetesterFactory.Create(chatClient);
    
    await builder
        .WithPrompt("Calculate 15% tip on $50")
        .ShouldCallFunctionWithParameters("calculate_tip", 
            new Dictionary<string, object?> 
            { 
                { "amount", 50 },
                { "percentage", 15 }
            })
        .AssertAsync();
}
```

### Example 9: Boolean Parameters

Verify functions with boolean parameters:

```csharp
[Fact]
public async Task TestBooleanParameters()
{
    var chatClient = // your IChatClient implementation
    var builder = DetesterFactory.Create(chatClient);
    
    await builder
        .WithPrompt("Search for flights to Paris with direct flights only")
        .ShouldCallFunctionWithParameters("search_flights", 
            new Dictionary<string, object?> 
            { 
                { "destination", "Paris" },
                { "direct_only", true }
            })
        .AssertAsync();
}
```

### Example 10: Null Parameters

Verify functions with optional null parameters:

```csharp
[Fact]
public async Task TestNullParameters()
{
    var chatClient = // your IChatClient implementation
    var builder = DetesterFactory.Create(chatClient);
    
    await builder
        .WithPrompt("Get user profile")
        .ShouldCallFunctionWithParameters("get_profile", 
            new Dictionary<string, object?> 
            { 
                { "user_id", "123" },
                { "include_private", null }
            })
        .AssertAsync();
}
```

## Best Practices

### 1. Be Specific with Prompts

Write clear, specific prompts that guide the model to call the expected functions:

```csharp
// ✅ Good - Clear and specific
.WithPrompt("Use the weather API to get the current temperature in Paris")

// ❌ Less ideal - Vague
.WithPrompt("Tell me about Paris")
```

### 2. Use Instructions for Consistent Behavior

Use system instructions to ensure the model consistently uses functions:

```csharp
await builder
    .WithInstruction("You are a weather assistant. Always use the get_weather function to retrieve weather information.")
    .WithPrompt("Is it raining in London?")
    .ShouldCallFunction("get_weather")
    .AssertAsync();
```

### 3. Test Parameter Variations

Test different parameter combinations to ensure robustness:

```csharp
// Test with celsius
await builder
    .WithPrompt("Weather in Paris in celsius")
    .ShouldCallFunctionWithParameters("get_weather", 
        new Dictionary<string, object?> { { "location", "Paris" }, { "units", "celsius" } })
    .AssertAsync();

// Test with fahrenheit
await builder
    .WithPrompt("Weather in Paris in fahrenheit")
    .ShouldCallFunctionWithParameters("get_weather", 
        new Dictionary<string, object?> { { "location", "Paris" }, { "units", "fahrenheit" } })
    .AssertAsync();
```

### 4. Combine Function and Text Assertions

Verify both the function call and the resulting response:

```csharp
await builder
    .WithPrompt("What's the weather in Paris?")
    .ShouldCallFunction("get_weather")
    .ShouldContainResponse("temperature")
    .ShouldContainResponse("Paris")
    .AssertAsync();
```

### 5. Order Independence

Function call assertions are order-independent. Detester matches them as they're found:

```csharp
// These are equivalent
.ShouldCallFunction("func1")
.ShouldCallFunction("func2")

// Same as
.ShouldCallFunction("func2")
.ShouldCallFunction("func1")
```

### 6. Parameter Matching is Case-Insensitive for Strings

String parameter values are compared case-insensitively:

```csharp
// This will match even if the model returns "PARIS" or "paris"
.ShouldCallFunctionWithParameters("get_weather", 
    new Dictionary<string, object?> { { "location", "Paris" } })
```

### 7. Verify Only What Matters

If you don't care about specific parameters, use `ShouldCallFunction` instead:

```csharp
// ✅ Good - Only verify function is called
.ShouldCallFunction("get_weather")

// ❌ Over-specific - May fail on minor variations
.ShouldCallFunctionWithParameters("get_weather", 
    new Dictionary<string, object?> { /* all parameters */ })
```

## Troubleshooting

### Issue: Function Not Called

**Error Message:**
```
Expected function 'get_weather' to be called, but no function calls were made.
```

**Possible Causes:**
1. The model didn't understand it should call a function
2. Functions aren't properly configured in `ChatOptions`
3. The prompt isn't clear enough

**Solutions:**
- Use clearer, more directive prompts
- Add system instructions guiding function usage
- Ensure functions are registered in `ChatOptions.Tools`

### Issue: Wrong Function Called

**Error Message:**
```
Expected function 'get_weather' to be called, but only these functions were called: 'get_temperature'
```

**Possible Causes:**
1. Similar function names causing confusion
2. Prompt ambiguity

**Solutions:**
- Be more specific in your prompt
- Use system instructions to clarify which function to use
- Check function descriptions in your function definitions

### Issue: Parameter Mismatch

**Error Message:**
```
Expected function 'get_weather' to be called with parameters (location=Paris), but it was not called with those parameters.
```

**Possible Causes:**
1. Model interpreted location differently (e.g., "Paris, France" vs "Paris")
2. Additional parameters were included
3. Parameter value types don't match

**Solutions:**
- Use `ShouldCallFunction` if exact parameters aren't critical
- Adjust expected parameters to match model behavior
- Verify parameter types match (string vs int, etc.)

### Issue: Case Sensitivity

String parameters are compared case-insensitively, but parameter names are case-sensitive:

```csharp
// ✅ This works - value is case-insensitive
{ "location", "paris" }  // Matches "Paris", "PARIS", etc.

// ❌ This fails - parameter name is case-sensitive
{ "Location", "Paris" }  // Won't match if model uses "location"
```

### Issue: Multiple Calls Not Matching

**Error Message:**
```
Expected function 'get_weather' to be called, but only these functions were called: 'get_weather'
```

This happens when you expect more calls than actually occurred.

**Solution:**
- Verify your prompt actually requests multiple operations
- Check if the model combined operations into a single call

### Issue: Extra Parameters

The model may include extra parameters not in your expectations. This is OK:

```csharp
// Model calls: { "location": "Paris", "units": "celsius", "include_forecast": true }
// Your expectation: { "location": "Paris", "units": "celsius" }
// Result: ✅ Match! Extra parameters are ignored
```

## Advanced Topics

### Testing with Mock Clients

For unit testing without actual API calls:

```csharp
using Microsoft.Extensions.AI;

var mockClient = new MockChatClient
{
    ResponseText = string.Empty,
    FunctionCallsToReturn =
    [
        new FunctionCallContent("call-123", "get_weather", 
            new Dictionary<string, object?> { { "location", "Paris" } })
    ]
};

var builder = DetesterFactory.Create(mockClient);

await builder
    .WithPrompt("What's the weather in Paris?")
    .ShouldCallFunction("get_weather")
    .AssertAsync();
```

### Configuring Functions in ChatOptions

To enable function calling with real AI models:

```csharp
using Microsoft.Extensions.AI;

var options = new ChatOptions
{
    Tools = 
    [
        AIFunctionFactory.Create(
            name: "get_weather",
            description: "Get weather information for a location",
            parameters: new { location = "", units = "celsius" },
            (string location, string units) => $"Weather in {location}: 20°{units}")
    ]
};

// Pass options when creating the builder or calling CompleteAsync
```

### Working with Complex Parameter Types

For complex objects, ensure proper serialization:

```csharp
var complexParams = new Dictionary<string, object?>
{
    { "location", new { city = "Paris", country = "France" } },
    { "preferences", new[] { "temperature", "humidity" } }
};

await builder
    .WithPrompt("Get detailed weather")
    .ShouldCallFunctionWithParameters("get_weather", complexParams)
    .AssertAsync();
```

## Error Messages Reference

| Error Message | Meaning | Solution |
|--------------|---------|----------|
| `Expected function '{name}' to be called, but no function calls were made.` | No functions were called | Improve prompt or add instructions |
| `Expected function '{name}' to be called, but only these functions were called: {actual}` | Wrong function called | Check prompt clarity or function names |
| `Expected function '{name}' to be called with parameters ({params}), but it was not called with those parameters.` | Parameters don't match | Verify parameter values or use ShouldCallFunction |
| `Function name cannot be null or whitespace.` | Invalid function name | Provide valid function name |
| `No prompts have been added.` | Missing prompts | Add at least one prompt with WithPrompt |

## Related Documentation

- [Main README](../README.md) - Getting started with Detester
- [Investigation Document](../FUNCTION_CALLING_INVESTIGATION.md) - Technical details and design decisions
- [Microsoft.Extensions.AI Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.ai) - Understanding the AI abstractions

## Support

If you encounter issues or have questions:

1. Check the [Troubleshooting](#troubleshooting) section
2. Review the [Examples](#examples) for similar use cases
3. Open an issue on [GitHub](https://github.com/sa-es-ir/detester/issues)

## Contributing

Contributions are welcome! Please see the main repository for contribution guidelines.
