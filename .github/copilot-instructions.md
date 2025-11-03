# GitHub Copilot Instructions for Detester

## Project Overview

Detester is a .NET library for building deterministic and reliable tests for AI applications. It provides a fluent builder API for testing AI responses with support for OpenAI, Azure OpenAI, and custom IChatClient implementations.

## Core Technologies

- **.NET 9.0**: Target framework
- **Microsoft.Extensions.AI**: Core abstraction for AI integrations
- **OpenAI SDK**: For OpenAI integration
- **Azure.AI.OpenAI**: For Azure OpenAI integration
- **xUnit**: Testing framework
- **StyleCop**: Code style enforcement

## Code Style and Conventions

### General Guidelines

- Follow C# naming conventions (PascalCase for public members, camelCase for private fields)
- Use XML documentation comments for all public APIs
- Follow StyleCop rules configured in `stylecop.json`
- Use nullable reference types appropriately
- Prefer explicit over implicit typing for clarity

### API Design

- Use fluent builder pattern for API methods (return `this` or interface type)
- Validate input parameters and throw appropriate exceptions with descriptive messages
- Use `ArgumentNullException` for null reference parameters
- Use `ArgumentException` for invalid parameter values
- Throw `DetesterException` for domain-specific errors

### Testing

- Write comprehensive unit tests using xUnit
- Use `[Fact]` for simple tests, `[Theory]` with `[InlineData]` for parameterized tests
- Follow Arrange-Act-Assert pattern
- Mock external dependencies (use `MockChatClient` for IChatClient)
- Test both success and failure scenarios

## Project Structure

```
/src
  /Detester                    - Main library with implementations
  /Detester.Abstraction        - Interfaces and base types
/test
  /Detester.Tests             - Unit tests
```

## Key Classes and Interfaces

### Core Components

- `IDetesterBuilder`: Main interface for fluent API
- `DetesterBuilder`: Implementation of the builder pattern
- `DetesterFactory`: Factory methods for creating builder instances
- `DetesterOptions`: Configuration options
- `DetesterException`: Custom exception type

### Factory Methods

- `CreateWithOpenAI(apiKey, modelName)`: Create builder for OpenAI
- `CreateWithAzureOpenAI(endpoint, apiKey, deploymentName)`: Create builder for Azure OpenAI
- `Create(options)`: Create builder from options
- `Create(chatClient)`: Create builder with custom chat client

### Builder Methods

- `WithPrompt(prompt)`: Add a single prompt
- `WithPrompts(params prompts)`: Add multiple prompts
- `ShouldContainResponse(expectedText)`: Add assertion for response content
- `AssertAsync(cancellationToken)`: Execute and validate

## Implementation Guidelines

### When Adding New Features

1. Start with interface definition in `Detester.Abstraction`
2. Add XML documentation comments
3. Implement in `Detester` project
4. Add factory methods if needed
5. Write comprehensive tests
6. Update README.md with examples

### When Fixing Bugs

1. Write a failing test that reproduces the issue
2. Fix the implementation
3. Ensure all existing tests still pass
4. Update documentation if needed

### Building and Testing

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run all tests
dotnet test
```

## Common Patterns

### Creating a New Builder Method

```csharp
/// <summary>
/// Description of what the method does.
/// </summary>
/// <param name="paramName">Parameter description.</param>
/// <returns>The builder instance for method chaining.</returns>
public IDetesterBuilder MethodName(string paramName)
{
    if (string.IsNullOrWhiteSpace(paramName))
    {
        throw new ArgumentException("Parameter cannot be null or whitespace.", nameof(paramName));
    }

    // Implementation logic
    
    return this;
}
```

### Error Handling

- Validate inputs early and fail fast
- Use descriptive error messages
- Include parameter names in exceptions
- Don't catch exceptions unless you can handle them meaningfully

## Dependencies

- Keep dependencies minimal and up to date
- Only add new dependencies when absolutely necessary
- Prefer Microsoft.Extensions.AI abstractions over direct SDK usage when possible

## Documentation

- Update README.md for any user-facing changes
- Include code examples in documentation
- Keep API reference section synchronized with actual API
- Document breaking changes clearly

## Example Usage Pattern

```csharp
// Typical test pattern
await builder
    .WithPrompt("User prompt")
    .ShouldContainResponse("expected text")
    .AssertAsync();

// Multiple prompts and assertions
await builder
    .WithPrompts("Prompt 1", "Prompt 2")
    .ShouldContainResponse("keyword1")
    .ShouldContainResponse("keyword2")
    .AssertAsync();
```

## Important Notes

- Response matching is case-insensitive
- All prompts must be provided before calling AssertAsync
- Method chaining is a core feature - maintain it in all builder methods
- The library should work with any IChatClient implementation

## CI/CD and Workflows

### Continuous Integration

The project uses GitHub Actions for CI. The workflow is defined in `.github/workflows/ci.yml` and runs on:
- Push to `main` branch
- Pull requests to `main` branch

The CI pipeline:
1. Checks out code
2. Sets up .NET 8 and .NET 9 SDKs
3. Restores dependencies with `dotnet restore`
4. Builds with `dotnet build --no-restore`
5. Runs tests with `dotnet test --no-build --verbosity normal`

### Running CI Locally

To replicate CI locally:
```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build --verbosity normal
```

## Security Best Practices

### API Keys and Secrets

- Never commit API keys or secrets to the repository
- Use environment variables for sensitive data (e.g., `OPENAI_API_KEY`, `AZURE_OPENAI_API_KEY`)
- In tests, use `Environment.GetEnvironmentVariable()` to access keys
- For local development, use user secrets or `.env` files (add to `.gitignore`)
- Document required environment variables in test files or README

### Secure Testing

- Mock external API calls in unit tests when possible
- Integration tests requiring real API keys should be clearly marked
- Consider using test API keys with limited scope for integration tests

## Troubleshooting

### Common Issues

**Build Failures:**
- Ensure you have both .NET 8 and .NET 9 SDKs installed
- Run `dotnet restore` before building
- Check StyleCop warnings - they may indicate code style issues

**Test Failures:**
- Verify API keys are set correctly in environment variables
- Check network connectivity for tests that make real API calls
- Review test output for specific assertion failures

**StyleCop Warnings:**
- XML documentation is required for all public APIs
- Follow C# naming conventions strictly
- Check `stylecop.json` for configured rules

**Dependency Issues:**
- Run `dotnet restore` to update packages
- Check for version conflicts in `.csproj` files
- Ensure Microsoft.Extensions.AI version compatibility

### Debug Tips

- Use `dotnet test --logger "console;verbosity=detailed"` for detailed test output
- Enable debug logging in tests with appropriate verbosity
- Use breakpoints in test methods to inspect builder state
- Review actual vs expected responses in assertion failures

## Quick Reference Commands

### Development
```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Build in Release mode
dotnet build -c Release

# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test
dotnet test --filter "FullyQualifiedName~TestMethodName"
```

### Code Quality
```bash
# Build with warnings as errors (CI simulation)
dotnet build /p:TreatWarningsAsErrors=true

# Clean build artifacts
dotnet clean

# Format code (if formatter is configured)
dotnet format
```

### Package Management
```bash
# Add package to specific project
dotnet add src/Detester/Detester.csproj package PackageName

# Update package
dotnet add package PackageName

# List packages
dotnet list package
```

## Release Process

The project uses semantic versioning (SemVer). Current version is tracked via git tags.

- Stable 1.0.0 release is planned after .NET 10 LTS
- Preview versions follow the pattern: `v1.0.0-preview.X`
- Always update version numbers in `.csproj` files before release
- Tag releases with `git tag -a vX.Y.Z -m "Release version X.Y.Z"`

## File Organization

```
/home/runner/work/detester/detester/
├── .github/
│   ├── copilot-instructions.md     - This file
│   └── workflows/
│       └── ci.yml                   - CI/CD pipeline
├── src/
│   ├── Detester/                    - Main implementation
│   └── Detester.Abstraction/        - Interfaces and abstractions
├── test/
│   └── Detester.Tests/              - Unit and integration tests
├── docs/
│   └── function-calling-guide.md    - Function calling documentation
├── Detester.sln                     - Solution file
├── Directory.Build.props            - Shared MSBuild properties
├── Directory.Build.targets          - Shared MSBuild targets
├── stylecop.json                    - StyleCop configuration
└── README.md                        - Main documentation
```
