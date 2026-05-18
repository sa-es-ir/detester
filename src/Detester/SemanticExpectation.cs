// Copyright (c) Detester. All rights reserved.

namespace Detester;

/// <summary>
/// Represents an expectation that the AI response is semantically similar to an expected text.
/// </summary>
internal sealed class SemanticExpectation
{
    required public string Expected { get; set; }

    public double MinScore { get; set; }
}
