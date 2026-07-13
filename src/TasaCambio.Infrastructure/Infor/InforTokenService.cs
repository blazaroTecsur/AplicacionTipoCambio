using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace TasaCambio.Infrastructure.Infor;

internal sealed class InforTokenService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly InforSettings _settings;
    private readonly ILogger<InforTokenService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private string? _tokenCache;
    private DateTime _tokenExpira = DateTime.MinValue;

    public InforTokenService(
        IHttpClientFactory httpClientFactory,
        IOptions<InforSettings> settings,
        ILogger<InforTokenService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> ObtenerTokenAsync(CancellationToken ct = default)
    {
        if (_tokenCache is not null && DateTime.UtcNow < _tokenExpira)
            return _tokenCache;

        await _lock.WaitAsync(ct);
        try
        {
            // Double-check después del lock
            if (_tokenCache is not null && DateTime.UtcNow < _tokenExpira)
                return _tokenCache;

            var client = _httpClientFactory.CreateClient("InforSsoClient");
            var url = $"{_settings.SsoBaseUrl.TrimEnd('/')}{_settings.TokenEndpoint}";

            var form = new FormUrlEncodedContent(
            [
                KeyValuePair.Create("grant_type",    "password"),
                KeyValuePair.Create("client_id",     _settings.ClientId),
                KeyValuePair.Create("client_secret", _settings.ClientSecret),
                KeyValuePair.Create("username",      _settings.ServiceAccountKey),
                KeyValuePair.Create("password",      _settings.ServiceAccountSecret),
            ]);

            using var response = await client.PostAsync(url, form, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            var root = doc.RootElement;
            var token      = root.GetProperty("access_token").GetString()!;
            var expiresIn  = root.TryGetProperty("expires_in", out var expProp) ? expProp.GetInt32() : 3600;

            _tokenCache  = token;
            _tokenExpira = DateTime.UtcNow.AddSeconds(expiresIn - 60); // renovar 60s antes

            _logger.LogDebug("[INFOR-SSO] Token obtenido. Expira en {Seg}s.", expiresIn);
            return _tokenCache;
        }
        finally
        {
            _lock.Release();
        }
    }
}
