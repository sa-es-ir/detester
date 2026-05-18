// Copyright (c) Detester. All rights reserved.

namespace Detester;

/// <summary>
/// Represents an expectation evaluated by an LLM acting as a judge against a natural-language criteria.
/// </summary>
internal sealed class JudgeExpectation
{
    required public string Criteria { get; set; }
}
