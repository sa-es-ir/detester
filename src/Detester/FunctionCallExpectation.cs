// Copyright (c) Detester. All rights reserved.

namespace Detester;

/// <summary>
/// Represents an expectation for a function call in an AI response.
/// </summary>
internal sealed class FunctionCallExpectation
{
    /// <summary>
    /// Gets or sets the name of the function that is expected to be called.
    /// </summary>
    required public string FunctionName { get; set; }

    /// <summary>
    /// Gets or sets the expected parameters for the function call.
    /// If null, parameters are not verified.
    /// </summary>
    public IDictionary<string, object?>? ExpectedParameters { get; set; }
}
