using System.Text.Json.Serialization;

namespace Infor.Abstractions.DTOs;

public sealed class IdoItemsResponse
{
    [JsonPropertyName("Name")]
    public string? Name { get; init; }

    [JsonPropertyName("Value")]
    public string? Value { get; init; }
}
