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

    [JsonPropertyName("Items")]
    public List<List<IdoItemsResponse>> Items { get; init; } = [];
}
