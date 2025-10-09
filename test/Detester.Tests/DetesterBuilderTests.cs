namespace Detester.Tests;

using Detester.Abstraction;
using Microsoft.Extensions.AI;

/// <summary>
/// Tests for the DetesterBuilder class.
/// </summary>
public class DetesterBuilderTests
{
    [Fact]
    public void Constructor_WithNullChatClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DetesterBuilder(null!));
    }

    [Fact]
    public void WithInstruction_WithNullInstruction_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithInstruction(null!));
    }

    [Fact]
    public void WithInstruction_WithEmptyInstruction_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithInstruction(string.Empty));
    }

    [Fact]
    public void WithInstruction_WithWhitespaceInstruction_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithInstruction("   "));
    }

    [Fact]
    public void WithInstruction_WithValidInstruction_ReturnsBuilder()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act
        var result = builder.WithInstruction("You are a helpful assistant.");

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IDetesterBuilder>(result);
    }

    [Fact]
    public async Task AssertAsync_WithInstruction_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "This is a helpful response"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithInstruction("You are a helpful assistant.")
            .WithPrompt("Test prompt")
            .ShouldContainResponse("helpful")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithInstructionAndMultiplePrompts_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Response following the instruction"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithInstruction("You are a helpful assistant.")
            .WithPrompt("First prompt")
            .WithPrompt("Second prompt")
            .ShouldContainResponse("Response")
            .ShouldContainResponse("instruction")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithInstructionBeforePrompts_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Instruction-based response"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithInstruction("Answer concisely.")
            .WithPrompt("What is AI?")
            .ShouldContainResponse("Instruction")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithoutInstruction_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Normal response"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPrompt("Test prompt")
            .ShouldContainResponse("Normal")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public void WithPrompt_WithNullPrompt_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithPrompt(null!));
    }

    [Fact]
    public void WithPrompt_WithEmptyPrompt_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithPrompt(string.Empty));
    }

    [Fact]
    public void WithPrompt_WithValidPrompt_ReturnsBuilder()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act
        var result = builder.WithPrompt("Test prompt");

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IDetesterBuilder>(result);
    }

    [Fact]
    public void WithPrompts_WithNullPrompts_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithPrompts(null!));
    }

    [Fact]
    public void WithPrompts_WithEmptyArray_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithPrompts(Array.Empty<string>()));
    }

    [Fact]
    public void WithPrompts_WithValidPrompts_ReturnsBuilder()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act
        var result = builder.WithPrompts("Prompt 1", "Prompt 2");

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IDetesterBuilder>(result);
    }

    [Fact]
    public void ShouldContainResponse_WithNullText_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.ShouldContainResponse(null!));
    }

    [Fact]
    public void ShouldContainResponse_WithEmptyText_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.ShouldContainResponse(string.Empty));
    }

    [Fact]
    public void ShouldContainResponse_WithValidText_ReturnsBuilder()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act
        var result = builder.ShouldContainResponse("Expected text");

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IDetesterBuilder>(result);
    }

    [Fact]
    public async Task AssertAsync_WithoutPrompts_ThrowsDetesterException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await Assert.ThrowsAsync<DetesterException>(() => builder.AssertAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AssertAsync_WithPrompt_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "This is a test response"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder.WithPrompt("Test prompt").AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithMatchingExpectation_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "This is a test response"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPrompt("Test prompt")
            .ShouldContainResponse("test response")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithNonMatchingExpectation_ThrowsDetesterException()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "This is a test response"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await Assert.ThrowsAsync<DetesterException>(() =>
            builder
                .WithPrompt("Test prompt")
                .ShouldContainResponse("missing text")
                .AssertAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AssertAsync_SupportsMethodChaining()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Response with expected keywords"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPrompt("First prompt")
            .WithPrompt("Second prompt")
            .ShouldContainResponse("expected")
            .ShouldContainResponse("keywords")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public void OrShouldContainResponse_WithNullText_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            builder.ShouldContainResponse("test").OrShouldContainResponse(null!));
    }

    [Fact]
    public void OrShouldContainResponse_WithEmptyText_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            builder.ShouldContainResponse("test").OrShouldContainResponse(string.Empty));
    }

    [Fact]
    public void OrShouldContainResponse_WithValidText_ReturnsBuilder()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act
        var result = builder
            .ShouldContainResponse("test")
            .OrShouldContainResponse("alternative");

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IDetesterBuilder>(result);
    }

    [Fact]
    public void OrShouldContainResponse_WithoutPriorAssertion_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            builder.OrShouldContainResponse("test"));
    }

    [Fact]
    public async Task AssertAsync_WithOrShouldContainResponse_FirstOptionMatches_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Response with expected keywords"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPrompt("Test prompt")
            .ShouldContainResponse("expected")
            .OrShouldContainResponse("alternative")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithOrShouldContainResponse_SecondOptionMatches_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Response with alternative keywords"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPrompt("Test prompt")
            .ShouldContainResponse("expected")
            .OrShouldContainResponse("alternative")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithOrShouldContainResponse_NoOptionMatches_ThrowsDetesterException()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Response with different keywords"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DetesterException>(() =>
            builder
                .WithPrompt("Test prompt")
                .ShouldContainResponse("expected")
                .OrShouldContainResponse("alternative")
                .AssertAsync(TestContext.Current.CancellationToken));

        Assert.Contains("expected' OR 'alternative", exception.Message);
    }

    [Fact]
    public async Task AssertAsync_WithMultipleOrOptions_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Response with keywords 2"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPrompt("First prompt")
            .WithPrompt("Second prompt")
            .ShouldContainResponse("expected")
            .OrShouldContainResponse("keywords 1")
            .OrShouldContainResponse("keywords 2")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithMixedAndOrAssertions_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Response with expected and alternative keywords"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPrompt("Test prompt")
            .ShouldContainResponse("Response")
            .ShouldContainResponse("expected")
            .OrShouldContainResponse("missing")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithMixedAndOrAssertions_MissingAndAssertion_ThrowsDetesterException()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Response with alternative keywords"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await Assert.ThrowsAsync<DetesterException>(() =>
            builder
                .WithPrompt("Test prompt")
                .ShouldContainResponse("missing")
                .ShouldContainResponse("expected")
                .OrShouldContainResponse("alternative")
                .AssertAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void WithInstructionFromFile_WithNullFilePath_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithInstructionFromFile(null!));
    }

    [Fact]
    public void WithInstructionFromFile_WithEmptyFilePath_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithInstructionFromFile(string.Empty));
    }

    [Fact]
    public void WithInstructionFromFile_WithWhitespaceFilePath_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithInstructionFromFile("   "));
    }

    [Fact]
    public void WithInstructionFromFile_WithInvalidFileType_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            builder.WithInstructionFromFile("TestFiles/invalid.json"));
        Assert.Contains("markdown (.md) or text (.txt)", exception.Message);
    }

    [Fact]
    public void WithInstructionFromFile_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
            builder.WithInstructionFromFile("TestFiles/nonexistent.txt"));
    }

    [Fact]
    public void WithInstructionFromFile_WithEmptyFile_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            builder.WithInstructionFromFile("TestFiles/empty.txt"));
    }

    [Fact]
    public void WithInstructionFromFile_WithValidTxtFile_ReturnsBuilder()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act
        var result = builder.WithInstructionFromFile("TestFiles/instruction.txt");

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IDetesterBuilder>(result);
    }

    [Fact]
    public void WithInstructionFromFile_WithValidMdFile_ReturnsBuilder()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act
        var result = builder.WithInstructionFromFile("TestFiles/instruction.md");

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IDetesterBuilder>(result);
    }

    [Fact]
    public async Task AssertAsync_WithInstructionFromTxtFile_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "This is a helpful response"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithInstructionFromFile("TestFiles/instruction.txt")
            .WithPrompt("Test prompt")
            .ShouldContainResponse("helpful")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithInstructionFromMdFile_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "This is a concise response"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithInstructionFromFile("TestFiles/instruction.md")
            .WithPrompt("Test prompt")
            .ShouldContainResponse("concise")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public void WithPromptFromFile_WithNullFilePath_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithPromptFromFile(null!));
    }

    [Fact]
    public void WithPromptFromFile_WithEmptyFilePath_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithPromptFromFile(string.Empty));
    }

    [Fact]
    public void WithPromptFromFile_WithWhitespaceFilePath_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithPromptFromFile("   "));
    }

    [Fact]
    public void WithPromptFromFile_WithInvalidFileType_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            builder.WithPromptFromFile("TestFiles/invalid.json"));
        Assert.Contains("markdown (.md) or text (.txt)", exception.Message);
    }

    [Fact]
    public void WithPromptFromFile_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
            builder.WithPromptFromFile("TestFiles/nonexistent.txt"));
    }

    [Fact]
    public void WithPromptFromFile_WithEmptyFile_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            builder.WithPromptFromFile("TestFiles/empty.txt"));
    }

    [Fact]
    public void WithPromptFromFile_WithValidTxtFile_ReturnsBuilder()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act
        var result = builder.WithPromptFromFile("TestFiles/prompt.txt");

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IDetesterBuilder>(result);
    }

    [Fact]
    public void WithPromptFromFile_WithValidMdFile_ReturnsBuilder()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act
        var result = builder.WithPromptFromFile("TestFiles/prompt.md");

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IDetesterBuilder>(result);
    }

    [Fact]
    public async Task AssertAsync_WithPromptFromTxtFile_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "The capital of France is Paris"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPromptFromFile("TestFiles/prompt.txt")
            .ShouldContainResponse("Paris")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithPromptFromMdFile_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "Quantum computing uses quantum bits"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPromptFromFile("TestFiles/prompt.md")
            .ShouldContainResponse("quantum")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithInstructionAndPromptFromFiles_CompletesSuccessfully()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "This is a helpful and concise response"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithInstructionFromFile("TestFiles/instruction.txt")
            .WithPromptFromFile("TestFiles/prompt.txt")
            .ShouldContainResponse("helpful")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public void ShouldCallFunction_WithNullFunctionName_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.ShouldCallFunction(null!));
    }

    [Fact]
    public void ShouldCallFunction_WithEmptyFunctionName_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.ShouldCallFunction(string.Empty));
    }

    [Fact]
    public void ShouldCallFunctionWithParameters_WithNullFunctionName_ThrowsArgumentException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);
        var parameters = new Dictionary<string, object?> { { "key", "value" } };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.ShouldCallFunctionWithParameters(null!, parameters));
    }

    [Fact]
    public void ShouldCallFunctionWithParameters_WithNullParameters_ThrowsArgumentNullException()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.ShouldCallFunctionWithParameters("function_name", null!));
    }

    [Fact]
    public async Task ShouldCallFunction_WhenFunctionIsCalled_Succeeds()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = string.Empty,
            FunctionCallsToReturn =
            [
                new FunctionCallContent("call-123", "get_weather", new Dictionary<string, object?> { { "location", "Paris" } })
            ]
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPrompt("What's the weather in Paris?")
            .ShouldCallFunction("get_weather")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldCallFunction_WhenFunctionIsNotCalled_ThrowsDetesterException()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "I don't know the weather",
            FunctionCallsToReturn = []
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DetesterException>(async () =>
            await builder
                .WithPrompt("What's the weather in Paris?")
                .ShouldCallFunction("get_weather")
                .AssertAsync(TestContext.Current.CancellationToken));

        Assert.Contains("Expected function 'get_weather' to be called", exception.Message);
        Assert.Contains("no function calls were made", exception.Message);
    }

    [Fact]
    public async Task ShouldCallFunction_WhenWrongFunctionIsCalled_ThrowsDetesterException()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = string.Empty,
            FunctionCallsToReturn =
            [
                new FunctionCallContent("call-123", "get_temperature", new Dictionary<string, object?>())
            ]
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DetesterException>(async () =>
            await builder
                .WithPrompt("What's the weather in Paris?")
                .ShouldCallFunction("get_weather")
                .AssertAsync(TestContext.Current.CancellationToken));

        Assert.Contains("Expected function 'get_weather' to be called", exception.Message);
        Assert.Contains("get_temperature", exception.Message);
    }

    [Fact]
    public async Task ShouldCallFunctionWithParameters_WhenParametersMatch_Succeeds()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = string.Empty,
            FunctionCallsToReturn =
            [
                new FunctionCallContent("call-123", "get_weather", new Dictionary<string, object?>
                {
                    { "location", "Paris" },
                    { "units", "celsius" }
                })
            ]
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPrompt("What's the weather in Paris in celsius?")
            .ShouldCallFunctionWithParameters("get_weather", new Dictionary<string, object?>
            {
                { "location", "Paris" },
                { "units", "celsius" }
            })
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldCallFunctionWithParameters_WhenParametersDontMatch_ThrowsDetesterException()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = string.Empty,
            FunctionCallsToReturn =
            [
                new FunctionCallContent("call-123", "get_weather", new Dictionary<string, object?>
                {
                    { "location", "London" },
                    { "units", "celsius" }
                })
            ]
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DetesterException>(async () =>
            await builder
                .WithPrompt("What's the weather in Paris?")
                .ShouldCallFunctionWithParameters("get_weather", new Dictionary<string, object?>
                {
                    { "location", "Paris" },
                    { "units", "celsius" }
                })
                .AssertAsync(TestContext.Current.CancellationToken));

        Assert.Contains("Expected function 'get_weather' to be called with parameters", exception.Message);
    }

    [Fact]
    public async Task ShouldCallFunction_WithMultipleFunctionCalls_MatchesCorrectly()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = string.Empty,
            FunctionCallsToReturn =
            [
                new FunctionCallContent("call-1", "get_weather", new Dictionary<string, object?> { { "location", "Paris" } }),
                new FunctionCallContent("call-2", "get_weather", new Dictionary<string, object?> { { "location", "London" } })
            ]
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPrompt("What's the weather in Paris and London?")
            .ShouldCallFunction("get_weather")
            .ShouldCallFunction("get_weather")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldCallFunction_CombinedWithTextAssertion_Succeeds()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "The weather is sunny",
            FunctionCallsToReturn =
            [
                new FunctionCallContent("call-123", "get_weather", new Dictionary<string, object?> { { "location", "Paris" } })
            ]
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPrompt("What's the weather in Paris?")
            .ShouldCallFunction("get_weather")
            .ShouldContainResponse("sunny")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldCallFunction_WithCaseInsensitiveStringParameters_Succeeds()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = string.Empty,
            FunctionCallsToReturn =
            [
                new FunctionCallContent("call-123", "get_weather", new Dictionary<string, object?>
                {
                    { "location", "PARIS" }
                })
            ]
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        await builder
            .WithPrompt("What's the weather?")
            .ShouldCallFunctionWithParameters("get_weather", new Dictionary<string, object?>
            {
                { "location", "paris" }
            })
            .AssertAsync(TestContext.Current.CancellationToken);
    }
}
