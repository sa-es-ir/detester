namespace Detester;

using Detester.Abstraction;
using Microsoft.Extensions.AI;

/// <summary>
/// Factory class for creating DetesterBuilder instances with configured AI clients.
/// </summary>
public static class DetesterFactory
{
    /// <summary>
    /// Creates a DetesterBuilder with a custom IChatClient.
    /// </summary>
    /// <param name="chatClient">The custom chat client.</param>
    /// <returns>A configured DetesterBuilder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when chatClient is null.</exception>
    public static IDetesterBuilder Create(IChatClient chatClient)
    {
        ArgumentNullException.ThrowIfNull(chatClient);

        return new DetesterBuilder(chatClient);
    }
}
