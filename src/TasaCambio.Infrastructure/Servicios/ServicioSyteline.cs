using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TasaCambio.Application.Comun.Interfaces;

namespace TasaCambio.Infrastructure.Servicios;

internal sealed class ServicioSyteline : IServicioSyteline
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ServicioSyteline> _logger;
    private readonly string _baseUrl;
    private readonly string _usuario;
    private readonly string _password;
    private readonly string _sistemaId;
    private readonly string _monedaBase;

    public ServicioSyteline(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<ServicioSyteline> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _baseUrl = config["Syteline:BaseUrl"] ?? throw new InvalidOperationException("'Syteline:BaseUrl' no configurado.");
        _usuario = config["Syteline:Usuario"] ?? throw new InvalidOperationException("'Syteline:Usuario' no configurado.");
        _password = config["Syteline:Password"] ?? throw new InvalidOperationException("'Syteline:Password' no configurado.");
        _sistemaId = config["Syteline:SistemaId"] ?? throw new InvalidOperationException("'Syteline:SistemaId' no configurado.");
        _monedaBase = config["Syteline:MonedaBase"] ?? "PEN";
    }

    public async Task<bool> RegistrarTasaCambioAsync(
        string codigoMoneda, DateOnly fecha, decimal compra, decimal venta,
        string usuario, CancellationToken ct = default)
    {
        try
        {
            var sessionId = await ObtenerSesionAsync(ct);
            if (sessionId is null)
            {
                _logger.LogWarning("[SYTELINE-IDO] No se pudo obtener sesión para {Moneda}/{Fecha}", codigoMoneda, fecha);
                return false;
            }

            return await ActualizarTasaCambioAsync(sessionId, codigoMoneda, fecha, compra, venta, usuario, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SYTELINE-IDO] Error inesperado al registrar {Moneda} para {Fecha}", codigoMoneda, fecha);
            return false;
        }
    }

    private async Task<string?> ObtenerSesionAsync(CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("SytelineIdoClient");
        var url = $"{_baseUrl.TrimEnd('/')}/ido/session";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = JsonContent.Create(new IdoSesionRequest(_usuario, _password, _sistemaId));

        var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("[SYTELINE-IDO] Sesión rechazada: {Status}", response.StatusCode);
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<IdoSesionResponse>(cancellationToken: ct);
        return result?.SessionId;
    }

    private async Task<bool> ActualizarTasaCambioAsync(
        string sessionId, string codigoMoneda, DateOnly fecha,
        decimal compra, decimal venta, string usuario, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("SytelineIdoClient");
        var url = $"{_baseUrl.TrimEnd('/')}/ido/updatecollection/SLCurrates";

        var item = new IdoSLCurratesItem(
            ToCurrCode: codigoMoneda,
            FromCurrCode: _monedaBase,
            EffDate: fecha.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-ddTHH:mm:ss"),
            BuyRate: compra,
            SellRate: venta,
            UserCode: usuario);

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Session", sessionId);
        request.Content = JsonContent.Create(new IdoUpdateCollectionRequest([item]));

        var response = await client.SendAsync(request, ct);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation(
                "[SYTELINE-IDO] {Moneda} registrado — Compra: {Compra} / Venta: {Venta} / Fecha: {Fecha}",
                codigoMoneda, compra, venta, fecha);
            return true;
        }

        var detalle = await response.Content.ReadAsStringAsync(ct);
        _logger.LogWarning(
            "[SYTELINE-IDO] Error al actualizar {Moneda}/{Fecha}: {Status} — {Detalle}",
            codigoMoneda, fecha, response.StatusCode, detalle);
        return false;
    }

    // ── DTOs internos para la API IDO de SyteLine ──────────────────────────

    private sealed record IdoSesionRequest(
        [property: JsonPropertyName("userid")] string UserId,
        [property: JsonPropertyName("password")] string Password,
        [property: JsonPropertyName("sysid")]   string SysId);

    private sealed record IdoSesionResponse(
        [property: JsonPropertyName("sessionid")] string SessionId);

    private sealed record IdoUpdateCollectionRequest(
        [property: JsonPropertyName("Items")] IdoSLCurratesItem[] Items);

    private sealed record IdoSLCurratesItem(
        string ToCurrCode,
        string FromCurrCode,
        string EffDate,
        decimal BuyRate,
        decimal SellRate,
        string UserCode,
        // 2 = Modified (SyteLine hace upsert en base a la clave del IDO)
        [property: JsonPropertyName("_ItemFlags")] int ItemFlags = 2);
}
