// Copyright (c) Detester. All rights reserved.

namespace Detester;

/// <summary>
/// Represents a single prompt and the assertions that are scoped to that prompt's response.
/// </summary>
internal sealed class PromptStep
{
    public string Prompt { get; set; } = string.Empty;

    public List<string> ExpectedResponses { get; } = [];

    public List<List<string>> OrResponseGroups { get; } = [];

    public List<string> UnexpectedResponses { get; } = [];

    public List<string> UnexpectedAnyResponses { get; } = [];

    public List<string> RegexPatterns { get; } = [];

    public List<string> ContainAllSubstrings { get; } = [];

    public List<List<string>> ContainAnyGroups { get; } = [];

    public List<EqualityExpectation> EqualityExpectations { get; } = [];

    public List<FunctionCallExpectation> ExpectedFunctionCalls { get; } = [];

    public List<string> NotExpectedFunctionCalls { get; } = [];

    public List<JsonExpectation> JsonExpectations { get; } = [];

    public List<SemanticExpectation> SemanticExpectations { get; } = [];

    public List<JudgeExpectation> JudgeExpectations { get; } = [];
}
