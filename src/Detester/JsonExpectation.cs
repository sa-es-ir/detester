// Copyright (c) Detester. All rights reserved.

namespace Detester;

using System.Text.Json;

/// <summary>
/// Represents an expectation for JSON deserialization in an AI response.
/// </summary>
internal sealed class JsonExpectation
{
    /// <summary>
    /// Gets or sets the type to deserialize the JSON response into.
    /// </summary>
    required public Type TargetType { get; set; }

    /// <summary>
    /// Gets or sets the JSON serializer options to use for deserialization.
    /// </summary>
    public JsonSerializerOptions? Options { get; set; }

    /// <summary>
    /// Gets or sets the validator function to apply to the deserialized object.
    /// </summary>
    public Delegate? Validator { get; set; }
}
