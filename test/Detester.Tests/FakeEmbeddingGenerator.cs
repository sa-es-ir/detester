// Copyright (c) Detester. All rights reserved.

namespace Detester.Tests;

using Microsoft.Extensions.AI;

/// <summary>
/// A deterministic <see cref="IEmbeddingGenerator{TInput, TEmbedding}"/> for testing semantic
/// similarity. The supplied map controls the vector returned for a given input text.
/// </summary>
public sealed class FakeEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly Func<string, float[]> map;

    public FakeEmbeddingGenerator(Func<string, float[]> map)
    {
        this.map = map;
    }

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var embeddings = new GeneratedEmbeddings<Embedding<float>>(
            values.Select(v => new Embedding<float>(map(v))));
        return Task.FromResult(embeddings);
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
