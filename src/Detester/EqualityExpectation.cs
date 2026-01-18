// Copyright (c) Detester. All rights reserved.

namespace Detester;

internal sealed class EqualityExpectation
{
    public string Expected { get; set; } = string.Empty;

    public StringComparison Comparison { get; set; }
}
