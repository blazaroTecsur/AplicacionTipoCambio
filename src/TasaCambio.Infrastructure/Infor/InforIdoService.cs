using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TasaCambio.Infrastructure.Infor;

internal sealed class InforIdoService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly InforSettings _settings;
    private readonly InforTokenService _tokenService;
    private readonly ILogger<InforIdoService> _logger;

    public InforIdoService(
        IHttpClientFactory httpClientFactory,
        IOptions<InforSettings> settings,
        InforTokenService tokenService,
        ILogger<InforIdoService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings          = settings.Value;
        _tokenService      = tokenService;
        _logger            = logger;
    }

    // ── Load (GET) ────────────────────────────────────────────────────────────

    public async Task<JsonElement> LoadAsync(
        string ido,
        string? props      = null,
        string? filter     = null,
        int    recordCap   = 0,
        string? orderBy    = null,
        CancellationToken ct = default)
    {
        var query = BuildQuery(props, filter, recordCap, orderBy);
        var url   = $"{_settings.IdoBaseUrl.TrimEnd('/')}/load/{ido}{query}";
        return await EjecutarGetAsync(url, ct);
    }

    // ── Insert (Action = 1) ───────────────────────────────────────────────────

    public async Task<JsonElement> InsertItemAsync(
        string ido,
        IEnumerable<IdoPropiedad> properties,
        bool refreshAfterSave = false,
        CancellationToken ct  = default)
    {
        var url     = $"{_settings.IdoBaseUrl.TrimEnd('/')}/update/{ido}";
        var payload = new UpdateCollectionRequest(new[]
        {
            new CambioItem(
                Action:           1,
                ItemId:           null,
                RefreshAfterSave: refreshAfterSave,
                Properties:       properties.ToArray())
        });
        return await EjecutarPostAsync(url, payload, ct);
    }

    // ── Update (Action = 2) ───────────────────────────────────────────────────

    public async Task<JsonElement> UpdateItemAsync(
        string ido,
        string itemId,
        IEnumerable<IdoPropiedad> properties,
        bool refreshAfterSave = false,
        CancellationToken ct  = default)
    {
        var url     = $"{_settings.IdoBaseUrl.TrimEnd('/')}/update/{ido}";
        var payload = new UpdateCollectionRequest(new[]
        {
            new CambioItem(
                Action:           2,
                ItemId:           itemId,
                RefreshAfterSave: refreshAfterSave,
                Properties:       properties.ToArray())
        });
        return await EjecutarPostAsync(url, payload, ct);
    }

    // ── HTTP helpers ──────────────────────────────────────────────────────────

    private async Task<JsonElement> EjecutarGetAsync(string url, CancellationToken ct)
    {
        var token  = await _tokenService.ObtenerTokenAsync(ct);
        var client = _httpClientFactory.CreateClient("InforIdoClient");

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request, ct);
        return await LeerRespuestaAsync(response, ct);
    }

    private async Task<JsonElement> EjecutarPostAsync(string url, object body, CancellationToken ct)
    {
        var token  = await _tokenService.ObtenerTokenAsync(ct);
        var client = _httpClientFactory.CreateClient("InforIdoClient");

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(body);

        using var response = await client.SendAsync(request, ct);
        return await LeerRespuestaAsync(response, ct);
    }

    private async Task<JsonElement> LeerRespuestaAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var contenido = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("[INFOR-IDO] {Status} — {Detalle}", response.StatusCode, contenido);
            response.EnsureSuccessStatusCode(); // lanza excepción con detalle
        }

        if (string.IsNullOrWhiteSpace(contenido))
            return default;

        return JsonDocument.Parse(contenido).RootElement;
    }

    private static string BuildQuery(string? props, string? filter, int recordCap, string? orderBy)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(props))      parts.Add($"properties={Uri.EscapeDataString(props)}");
        if (!string.IsNullOrWhiteSpace(filter))     parts.Add($"filter={Uri.EscapeDataString(filter)}");
        if (recordCap > 0)                          parts.Add($"recordCap={recordCap}");
        if (!string.IsNullOrWhiteSpace(orderBy))    parts.Add($"orderBy={Uri.EscapeDataString(orderBy)}");
        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }

    // ── DTOs internos de IDO ──────────────────────────────────────────────────

    internal sealed record IdoPropiedad(
        [property: JsonPropertyName("Name")]     string Name,
        [property: JsonPropertyName("Value")]    string Value,
        [property: JsonPropertyName("Modified")] bool   Modified = true,
        [property: JsonPropertyName("IsNull")]   bool   IsNull   = false);

    private sealed record UpdateCollectionRequest(
        [property: JsonPropertyName("Changes")] CambioItem[] Changes);

    private sealed record CambioItem(
        [property: JsonPropertyName("Action")]           int              Action,
        [property: JsonPropertyName("ItemId")]           string?          ItemId,
        [property: JsonPropertyName("RefreshAfterSave")] bool             RefreshAfterSave,
        [property: JsonPropertyName("Properties")]       IdoPropiedad[]   Properties);
}
