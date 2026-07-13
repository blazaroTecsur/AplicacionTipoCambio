using System.Text.Json.Serialization;

namespace Infor.Abstractions.DTOs;

public sealed class IdoProperty
{
    [JsonPropertyName("IsNull")]
    public bool IsNull { get; init; } = false;

    [JsonPropertyName("Modified")]
    public bool Modified { get; init; } = true;

    [JsonPropertyName("Name")]
    public required string Name { get; init; }

    [JsonPropertyName("Value")]
    public string? Value { get; init; }
}
