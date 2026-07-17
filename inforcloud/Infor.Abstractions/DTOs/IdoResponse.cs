using System.Text.Json.Serialization;

namespace Infor.Abstractions.DTOs;

public sealed class IdoResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("bookmark")]
    public string? Bookmark { get; init; }

    [JsonPropertyName("moreRowsExist")]
    public bool MoreRowsExist { get; init; }

    // Cada elemento es un objeto plano { "PropName": "value", ... }
    [JsonPropertyName("Items")]
    public List<Dictionary<string, string?>> Items { get; init; } = [];
}
