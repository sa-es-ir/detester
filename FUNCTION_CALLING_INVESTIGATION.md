# Investigation & Planning: Function/Tool Call Verification in Detester

## Investigation Summary

### Microsoft.Extensions.AI Architecture

After thorough investigation of the Microsoft.Extensions.AI library (version 9.1.0-preview.1.25064.3), here are the key findings:

#### 1. How Function/Tool Calls Work

**Response Structure:**
- `ChatCompletion` contains a `Message` property of type `ChatMessage`
- `ChatMessage` has a `Contents` property which is an `IList<AIContent>`
- `AIContent` is a base class with several derived types including:
  - `TextContent` - for text responses
  - `FunctionCallContent` - for function/tool calls
  - `FunctionResultContent` - for function execution results
  - `ImageContent`, `AudioContent`, `DataContent`, etc.

**FunctionCallContent Properties:**
```csharp
- CallId: string - Unique identifier for the function call
- Name: string - Name of the function being called
- Arguments: IDictionary<string, object?> - Function parameters
- Exception: Exception - Any exception that occurred
- RawRepresentation: object - Original response from provider
- AdditionalProperties: AdditionalPropertiesDictionary
```

**FunctionResultContent Properties:**
```csharp
- CallId: string - Matches the CallId from FunctionCallContent
- Name: string - Name of the function
- Result: object - The result of the function execution
- Exception: Exception - Any exception that occurred
- RawRepresentation: object
- AdditionalProperties: AdditionalPropertiesDictionary
```

#### 2. How to Access Function Calls

When the AI model decides to call a function/tool:
1. The response will have `ChatMessage.Contents` containing one or more `FunctionCallContent` items
2. `ChatMessage.Text` will typically be null when only tool calls are present
3. Multiple function calls can be returned in a single response

Example:
```csharp
var response = await chatClient.CompleteAsync(chatHistory, options);
foreach (var content in response.Message.Contents)
{
    if (content is FunctionCallContent functionCall)
    {
        // Access: functionCall.Name, functionCall.CallId, functionCall.Arguments
    }
}
```

#### 3. Configuring Tools in ChatOptions

To enable function calling, tools must be provided in `ChatOptions`:
```csharp
var options = new ChatOptions
{
    Tools = [AIFunctionFactory.Create(...), ...]
};
```

### Feasibility Assessment

✅ **FEASIBLE** - The Microsoft.Extensions.AI library provides complete access to function call information through the response structure.

**What Information Can Be Verified:**
1. ✅ Function/tool name called
2. ✅ Function call ID
3. ✅ Function arguments (parameter names and values)
4. ✅ Multiple function calls in a single response
5. ✅ Presence of exceptions
6. ✅ Function results (if available in response)

**Limitations Identified:**
1. Function calls appear in `ChatMessage.Contents`, not as separate properties
2. A response may contain both text and function calls
3. Multiple function calls can occur in one response
4. The library doesn't automatically execute functions - that's up to the application

## Proposed API Design

### Basic Function Call Verification

```csharp
// Verify a function was called
await builder
    .WithPrompt("What is the weather in Paris?")
    .ShouldCallFunction("get_weather")
    .AssertAsync();
```

### Function Call with Parameter Verification

```csharp
// Verify specific parameters
await builder
    .WithPrompt("What is the weather in Paris?")
    .ShouldCallFunctionWithParameters("get_weather", 
        new Dictionary<string, object?> 
        { 
            { "location", "Paris" },
            { "units", "celsius" }
        })
    .AssertAsync();
```

### Multiple Function Calls

```csharp
// Verify multiple functions called
await builder
    .WithPrompt("What's the weather in Paris and London?")
    .ShouldCallFunction("get_weather")  // Called at least once
    .ShouldCallFunction("get_weather")  // Called at least twice
    .AssertAsync();
```

Or with count specification:
```csharp
await builder
    .WithPrompt("What's the weather in Paris and London?")
    .ShouldCallFunction("get_weather", times: 2)
    .AssertAsync();
```

### Combined Response and Function Call Verification

```csharp
await builder
    .WithPrompt("What is the capital of France?")
    .ShouldCallFunction("get_capital")
    .ShouldContainResponse("Paris")  // After function result is included
    .AssertAsync();
```

## Implementation Plan

### Phase 1: Core Infrastructure (Minimal Changes)

1. **Add new interface methods to `IDetesterBuilder`:**
   ```csharp
   IDetesterBuilder ShouldCallFunction(string functionName);
   IDetesterBuilder ShouldCallFunctionWithParameters(string functionName, IDictionary<string, object?> expectedParameters);
   ```

2. **Update `DetesterBuilder` implementation:**
   - Add field: `List<FunctionCallExpectation> expectedFunctionCalls`
   - Implement function call verification methods
   - Modify `AssertAsync` to check function calls in `response.Message.Contents`

3. **Create helper class `FunctionCallExpectation`:**
   ```csharp
   internal class FunctionCallExpectation
   {
       public string FunctionName { get; set; }
       public IDictionary<string, object?>? ExpectedParameters { get; set; }
   }
   ```

### Phase 2: Advanced Features (Future Enhancement)

1. Fluent parameter builder API
2. Parameter value matchers (contains, regex, type checks)
3. Function call ordering verification
4. Function result verification
5. Verification of function call count

### Technical Implementation Details

**In `DetesterBuilder.AssertAsync`:**

```csharp
// After getting response from chatClient.CompleteAsync()
foreach (var content in response.Message.Contents)
{
    if (content is FunctionCallContent functionCall)
    {
        // Check against expectedFunctionCalls
        // Verify function name matches
        // Verify parameters match (if specified)
    }
}

// Throw DetesterException if expectations not met
```

**Error Messages:**
- "Expected function '{name}' to be called, but no function calls were made."
- "Expected function '{name}' to be called, but only '{actual}' was called."
- "Function '{name}' was called with incorrect parameters. Expected {expected}, got {actual}."
- "Expected parameter '{param}' to be '{expected}', but was '{actual}'."

## Edge Cases and Considerations

### 1. Multiple Function Calls in Single Response
**Solution:** Track matched function calls to support multiple expectations for same function

### 2. Response Contains Both Text and Function Calls
**Solution:** Check both `TextContent` and `FunctionCallContent` independently

### 3. Async/Streaming Responses
**Limitation:** Current implementation uses `CompleteAsync` (non-streaming). Streaming support would require future enhancement.

### 4. Function Execution in Tests
**Consideration:** Detester verifies function CALLS, not execution results. Users must:
- Configure `ChatOptions.Tools` with actual functions OR
- Use mock implementations that don't execute but allow model to make calls

### 5. Provider Differences
**Note:** Different AI providers (OpenAI, Azure, etc.) handle function calling differently, but Microsoft.Extensions.AI abstracts this. We rely on the abstraction.

### 6. No Function Calls Expected
**Solution:** Current behavior (text-only verification) remains unchanged

## Testing Strategy

### Unit Tests to Add:

1. **Basic Function Call Verification:**
   - Test that function name is correctly verified
   - Test failure when wrong function called
   - Test failure when no function called

2. **Parameter Verification:**
   - Test exact parameter match
   - Test missing parameter detection
   - Test extra parameter handling
   - Test parameter value mismatch

3. **Multiple Function Calls:**
   - Test multiple calls to same function
   - Test multiple calls to different functions
   - Test order-independent matching

4. **Combined Verification:**
   - Test function call + text response
   - Test function call + response assertion

5. **Edge Cases:**
   - Empty function name
   - Null parameters
   - Complex parameter types (nested objects, arrays)

### Test Infrastructure:

**Enhanced `MockChatClient`:**
```csharp
public class MockChatClient : IChatClient
{
    public List<FunctionCallContent> FunctionCallsToReturn { get; set; }
    
    public Task<ChatCompletion> CompleteAsync(...)
    {
        var contents = new List<AIContent>();
        
        // Add text if ResponseText is set
        if (!string.IsNullOrEmpty(ResponseText))
        {
            contents.Add(new TextContent(ResponseText));
        }
        
        // Add function calls
        contents.AddRange(FunctionCallsToReturn);
        
        var message = new ChatMessage(ChatRole.Assistant, contents);
        return Task.FromResult(new ChatCompletion(message));
    }
}
```

## Documentation Updates

### README.md sections to add:

1. **Function/Tool Call Verification** section under "Advanced Usage"
2. **API Reference** updates for new methods
3. Examples showing:
   - Basic function call verification
   - Parameter verification
   - Combined text and function verification

## Breaking Changes

**None** - This is a pure addition to the API. Existing functionality remains unchanged.

## Dependencies

**No new dependencies required** - Uses existing Microsoft.Extensions.AI types.

## Migration Path

Not applicable - this is a new feature, no migration needed.

## Performance Considerations

- Minimal overhead - just iterating through `Message.Contents` list
- Parameter comparison is done once per response
- No significant performance impact expected

## Security Considerations

- Parameter values in error messages could expose sensitive data
- Consider sanitizing parameter values in error messages (future enhancement)

## Alternatives Considered

### Alternative 1: Separate Tool Assertion Method
```csharp
await builder
    .WithPrompt("...")
    .AssertAsync()
    .AssertFunctionCalled("function_name");
```
**Rejected:** Breaks fluent interface pattern and makes chaining with response assertions awkward.

### Alternative 2: Callback-based Verification
```csharp
await builder
    .WithPrompt("...")
    .ShouldCallFunction(call => 
    {
        Assert.Equal("get_weather", call.Name);
        Assert.Equal("Paris", call.Arguments["location"]);
    })
    .AssertAsync();
```
**Deferred:** More flexible but more complex. Could be added later as advanced feature.

### Alternative 3: Fluent Sub-builder Pattern
```csharp
await builder
    .WithPrompt("...")
    .ExpectFunctionCall()
        .Named("get_weather")
        .WithParameter("location", "Paris")
        .WithParameter("units", "celsius")
    .And()
    .ShouldContainResponse("temperature")
    .AssertAsync();
```
**Deferred:** Most expressive but adds complexity. Consider for v2.

## Recommendation

**Proceed with Phase 1 implementation** using the simple flat API:
- `ShouldCallFunction(string functionName)`
- `ShouldCallFunctionWithParameters(string functionName, IDictionary<string, object?> parameters)`

This provides immediate value while maintaining the library's simple, intuitive API design. Advanced features can be added in future iterations based on user feedback.

## Success Criteria

1. ✅ Users can verify function/tool calls in test code
2. ✅ Parameter verification works for simple types (string, int, bool)
3. ✅ Clear error messages when expectations not met
4. ✅ Works with existing text response verification
5. ✅ All unit tests pass
6. ✅ Documentation updated with examples
7. ✅ No breaking changes to existing API
8. ✅ Consistent with existing code style and patterns

## Timeline Estimate

- Investigation & Planning: ✅ Complete
- Phase 1 Implementation: 2-4 hours
  - Interface updates: 15 minutes
  - Implementation: 1-2 hours
  - Unit tests: 1-2 hours
  - Documentation: 30 minutes
- Testing & Refinement: 30 minutes

**Total Estimate:** 3-5 hours for complete Phase 1 implementation

## Next Steps

This investigation concludes that implementing function/tool call verification is **feasible and straightforward**. The Microsoft.Extensions.AI library provides all necessary information through the `FunctionCallContent` type in the response's `Contents` collection.

The recommended approach maintains consistency with Detester's existing fluent API while providing powerful verification capabilities for function calling scenarios.

Ready for implementation approval and Phase 1 development.
