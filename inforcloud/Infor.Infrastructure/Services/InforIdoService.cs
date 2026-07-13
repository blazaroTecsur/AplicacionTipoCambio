using Infor.Abstractions.DTOs;
using Infor.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infor.Infrastructure.Services;

internal sealed class InforIdoService : IInforIdoService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly InforSettings _settings;
    private readonly IInforTokenService _tokenService;
    private readonly ILogger<InforIdoService> _logger;

    public InforIdoService(
        IHttpClientFactory httpClientFactory,
        IOptions<InforSettings> settings,
        IInforTokenService tokenService,
        ILogger<InforIdoService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings          = settings.Value;
        _tokenService      = tokenService;
        _logger            = logger;
    }

    public async Task<IdoResponse> LoadAsync(
        string ido, string? properties = null, string? filter = null,
        int recordCap = 0, string? orderBy = null, CancellationToken ct = default)
    {
        var query = BuildQuery(properties, filter, recordCap, orderBy);
        var url   = $"{_settings.IdoBaseUrl.TrimEnd('/')}/load/{ido}{query}";
        return await EjecutarGetAsync(url, ct);
    }

    public async Task<IdoResponse> InsertItemAsync(
        string ido, IEnumerable<IdoProperty> properties,
        bool refreshAfterSave = false, CancellationToken ct = default)
    {
        var url     = $"{_settings.IdoBaseUrl.TrimEnd('/')}/update/{ido}";
        var payload = new UpdateCollectionRequest(new[]
        {
            new CambioItem(1, null, refreshAfterSave, properties.ToArray())
        });
        return await EjecutarPostAsync(url, payload, ct);
    }

    public async Task<IdoResponse> UpdateItemAsync(
        string ido, string itemId, IEnumerable<IdoProperty> properties,
        bool refreshAfterSave = false, CancellationToken ct = default)
    {
        var url     = $"{_settings.IdoBaseUrl.TrimEnd('/')}/update/{ido}";
        var payload = new UpdateCollectionRequest(new[]
        {
            new CambioItem(2, itemId, refreshAfterSave, properties.ToArray())
        });
        return await EjecutarPostAsync(url, payload, ct);
    }

    private async Task<IdoResponse> EjecutarGetAsync(string url, CancellationToken ct)
    {
        var token  = await _tokenService.ObtenerTokenAsync(ct);
        var client = _httpClientFactory.CreateClient("InforIdoClient");

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request, ct);
        return await LeerRespuestaAsync(response, ct);
    }

    private async Task<IdoResponse> EjecutarPostAsync(string url, object body, CancellationToken ct)
    {
        var token  = await _tokenService.ObtenerTokenAsync(ct);
        var client = _httpClientFactory.CreateClient("InforIdoClient");

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(body);

        using var response = await client.SendAsync(request, ct);
        return await LeerRespuestaAsync(response, ct);
    }

    private async Task<IdoResponse> LeerRespuestaAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var contenido = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("[INFOR-IDO] {Status} — {Detalle}", response.StatusCode, contenido);
            response.EnsureSuccessStatusCode();
        }

        if (string.IsNullOrWhiteSpace(contenido))
            return new IdoResponse { Success = true };

        return JsonSerializer.Deserialize<IdoResponse>(contenido, JsonOpts) ?? new IdoResponse { Success = true };
    }

    private static string BuildQuery(string? props, string? filter, int recordCap, string? orderBy)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(props))   parts.Add($"properties={Uri.EscapeDataString(props)}");
        if (!string.IsNullOrWhiteSpace(filter))  parts.Add($"filter={Uri.EscapeDataString(filter)}");
        if (recordCap > 0)                        parts.Add($"recordCap={recordCap}");
        if (!string.IsNullOrWhiteSpace(orderBy)) parts.Add($"orderBy={Uri.EscapeDataString(orderBy)}");
        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private sealed record UpdateCollectionRequest(
        [property: JsonPropertyName("Changes")] CambioItem[] Changes);

    private sealed record CambioItem(
        [property: JsonPropertyName("Action")]           int           Action,
        [property: JsonPropertyName("ItemId")]           string?       ItemId,
        [property: JsonPropertyName("RefreshAfterSave")] bool          RefreshAfterSave,
        [property: JsonPropertyName("Properties")]       IdoProperty[] Properties);
}
