// Copyright (c) Detester. All rights reserved.

namespace Detester.Tests;

using System.Text.Json;
using Detester.Abstraction;

/// <summary>
/// Tests for JSON validation feature in DetesterBuilder.
/// </summary>
public class DetesterBuilderJsonTests
{
    [Fact]
    public void ShouldHaveJsonOfType_WithNullOptions_ReturnsBuilder()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act
        var result = builder.ShouldHaveJsonOfType<User>();

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IDetesterBuilder>(result);
    }

    [Fact]
    public void ShouldHaveJsonOfType_WithNullValidator_ReturnsBuilder()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var result = builder.ShouldHaveJsonOfType<User>(options);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IDetesterBuilder>(result);
    }

    [Fact]
    public void ShouldHaveJsonOfType_WithValidator_ReturnsBuilder()
    {
        // Arrange
        var mockClient = new MockChatClient();
        var builder = new DetesterBuilder(mockClient);

        // Act
        var result = builder.ShouldHaveJsonOfType<User>(null, user => user.Age > 18);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IDetesterBuilder>(result);
    }

    [Fact]
    public async Task AssertAsync_WithValidJson_Succeeds()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = """
            {
                "firstName": "Joe",
                "lastName": "Doe",
                "age": 35,
                "joinDate": "2025-11-03"
            }
            """
        };
        var builder = new DetesterBuilder(mockClient);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act & Assert
        await builder
            .WithPrompt("Who is the last user joined?")
            .ShouldHaveJsonOfType<User>(options)
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithValidJsonAndCaseInsensitiveOptions_Succeeds()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = """
            {
                "firstname": "Joe",
                "lastname": "Doe",
                "age": 35,
                "joindate": "2025-11-03"
            }
            """
        };
        var builder = new DetesterBuilder(mockClient);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act & Assert
        await builder
            .WithPrompt("Who is the last user joined?")
            .ShouldHaveJsonOfType<User>(options)
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithValidJsonAndPassingValidator_Succeeds()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = """
            {
                "firstName": "Joe",
                "lastName": "Doe",
                "age": 35,
                "joinDate": "2025-11-03"
            }
            """
        };
        var builder = new DetesterBuilder(mockClient);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act & Assert
        await builder
            .WithPrompt("Who is the last user joined?")
            .ShouldHaveJsonOfType<User>(options, user => user.Age > 30 && user.FirstName!.Contains("Jo"))
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithValidJsonButFailingValidator_ThrowsDetesterException()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = """
            {
                "firstName": "Joe",
                "lastName": "Doe",
                "age": 25,
                "joinDate": "2025-11-03"
            }
            """
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DetesterException>(async () =>
            await builder
                .WithPrompt("Who is the last user joined?")
                .ShouldHaveJsonOfType<User>(null, user => user.Age > 30)
                .AssertAsync(TestContext.Current.CancellationToken));

        Assert.Contains("validation failed", exception.Message);
        Assert.Contains("User", exception.Message);
    }

    [Fact]
    public async Task AssertAsync_WithInvalidJson_ThrowsDetesterException()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "This is not valid JSON"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DetesterException>(async () =>
            await builder
                .WithPrompt("Who is the last user joined?")
                .ShouldHaveJsonOfType<User>()
                .AssertAsync(TestContext.Current.CancellationToken));

        Assert.Contains("Failed to deserialize", exception.Message);
        Assert.Contains("User", exception.Message);
        Assert.NotNull(exception.InnerException);
        Assert.IsType<JsonException>(exception.InnerException);
    }

    [Fact]
    public async Task AssertAsync_WithMalformedJson_ThrowsDetesterException()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = """
            {
                "firstName": "Joe",
                "lastName": "Doe",
                "age": 35,
                "joinDate": "not-a-valid-date"
            }
            """
        };
        var builder = new DetesterBuilder(mockClient);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DetesterException>(async () =>
            await builder
                .WithPrompt("Who is the last user joined?")
                .ShouldHaveJsonOfType<User>(options)
                .AssertAsync(TestContext.Current.CancellationToken));

        Assert.Contains("Failed to deserialize", exception.Message);
    }

    [Fact]
    public async Task AssertAsync_WithMultipleJsonExpectations_Succeeds()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = """
            {
                "firstName": "Joe",
                "lastName": "Doe",
                "age": 35,
                "joinDate": "2025-11-03"
            }
            """
        };
        var builder = new DetesterBuilder(mockClient);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act & Assert
        await builder
            .WithPrompt("Who is the last user joined?")
            .ShouldHaveJsonOfType<User>(options, user => user.Age > 18)
            .ShouldHaveJsonOfType<User>(options, user => user.LastName == "Doe")
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithJsonAndTextAssertions_Succeeds()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = """
            {
                "firstName": "Joe",
                "lastName": "Doe",
                "age": 35,
                "joinDate": "2025-11-03"
            }
            """
        };
        var builder = new DetesterBuilder(mockClient);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act & Assert
        await builder
            .WithPrompt("Who is the last user joined?")
            .ShouldContainResponse("Joe")
            .ShouldHaveJsonOfType<User>(options, user => user.Age > 30)
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithComplexValidator_Succeeds()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = """
            {
                "name": "Laptop",
                "price": 999.99,
                "inStock": true
            }
            """
        };
        var builder = new DetesterBuilder(mockClient);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act & Assert
        await builder
            .WithPrompt("Get product details")
            .ShouldHaveJsonOfType<Product>(options, product =>
                product.Price > 500 &&
                product.Price < 2000 &&
                product.InStock &&
                !string.IsNullOrEmpty(product.Name))
            .AssertAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAsync_WithNullDeserializationResult_ThrowsDetesterException()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = "null"
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DetesterException>(async () =>
            await builder
                .WithPrompt("Get user data")
                .ShouldHaveJsonOfType<User>()
                .AssertAsync(TestContext.Current.CancellationToken));

        Assert.Contains("resulted in null", exception.Message);
    }

    [Fact]
    public async Task AssertAsync_WithEmptyResponse_ThrowsDetesterException()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = string.Empty
        };
        var builder = new DetesterBuilder(mockClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DetesterException>(async () =>
            await builder
                .WithPrompt("Get user data")
                .ShouldHaveJsonOfType<User>()
                .AssertAsync(TestContext.Current.CancellationToken));

        Assert.Contains("Failed to deserialize", exception.Message);
    }

    [Fact]
    public async Task AssertAsync_WithJsonInCodeBlock_Succeeds()
    {
        // Arrange
        var mockClient = new MockChatClient
        {
            ResponseText = """
            ```json
            {
                "firstName": "Joe",
                "lastName": "Doe",
                "age": 35,
                "joinDate": "2025-11-03"
            }
            ```
            """
        };
        var builder = new DetesterBuilder(mockClient);

        // This test should fail because the response includes markdown code blocks
        // In a real implementation, you might want to extract JSON from code blocks
        // For now, this demonstrates that raw JSON is expected
        var exception = await Assert.ThrowsAsync<DetesterException>(async () =>
            await builder
                .WithPrompt("Who is the last user joined?")
                .ShouldHaveJsonOfType<User>()
                .AssertAsync(TestContext.Current.CancellationToken));

        Assert.Contains("Failed to deserialize", exception.Message);
    }

    /// <summary>
    /// User class for testing JSON deserialization.
    /// </summary>
    private class User
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public int Age { get; set; }

        public DateTime JoinDate { get; set; }
    }

    /// <summary>
    /// Product class for testing JSON deserialization.
    /// </summary>
    private class Product
    {
        public string? Name { get; set; }

        public decimal Price { get; set; }

        public bool InStock { get; set; }
    }
}
