using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TasaCambio.Application.Comun.Interfaces;
using TasaCambio.Application.Sbs;

namespace TasaCambio.Infrastructure.Servicios;

internal sealed class ServicioSbs : IServicioSbs
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ServicioSbs> _logger;
    private readonly string _urlXml;
    private readonly string _urlHtml;

    public ServicioSbs(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<ServicioSbs> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _urlXml = config["Sbs:UrlXml"] ?? throw new InvalidOperationException("Configuración 'Sbs:UrlXml' no encontrada.");
        _urlHtml = config["Sbs:UrlHtml"] ?? throw new InvalidOperationException("Configuración 'Sbs:UrlHtml' no encontrada.");
    }

    public Task<SbsTasaCambioDto?> ObtenerTasaCambioAsync(string codigoMoneda, DateOnly fecha, CancellationToken ct = default)
    {
        return codigoMoneda.ToUpperInvariant() switch
        {
            "USD" => ObtenerUsdDesdeXmlAsync(ct),
            "EUR" => ObtenerEurDesdeHtmlAsync(ct),
            _ => Task.FromResult<SbsTasaCambioDto?>(null)
        };
    }

    private async Task<SbsTasaCambioDto?> ObtenerUsdDesdeXmlAsync(CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("SbsXmlClient");
            var xml = await client.GetStringAsync(_urlXml, ct);

            var doc = XDocument.Parse(xml);

            var fechaStr  = doc.Descendants().FirstOrDefault(e => e.Name.LocalName.Equals("fecha",  StringComparison.OrdinalIgnoreCase))?.Value?.Trim();
            var compraStr = doc.Descendants().FirstOrDefault(e => e.Name.LocalName.Equals("compra", StringComparison.OrdinalIgnoreCase))?.Value?.Trim();
            var ventaStr  = doc.Descendants().FirstOrDefault(e => e.Name.LocalName.Equals("venta",  StringComparison.OrdinalIgnoreCase))?.Value?.Trim();

            if (string.IsNullOrWhiteSpace(compraStr) || string.IsNullOrWhiteSpace(ventaStr))
            {
                _logger.LogWarning("[SBS-XML] No se encontraron valores compra/venta en el XML.");
                return null;
            }

            if (!TryParseFechaSbs(fechaStr, out var fecha))
            {
                _logger.LogWarning("[SBS-XML] No se pudo parsear la fecha '{Fecha}' del XML.", fechaStr);
                return null;
            }

            _logger.LogDebug("[SBS-XML] Fecha SBS: {Fecha} | Compra: {Compra} | Venta: {Venta}", fecha, compraStr, ventaStr);

            return new SbsTasaCambioDto
            {
                CodigoMoneda      = "USD",
                DescripcionMoneda = "Dólar Americano",
                Fecha             = fecha,
                ValorCompra       = NormalizarDecimal(compraStr),
                ValorVenta        = NormalizarDecimal(ventaStr)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SBS-XML] Error al obtener tipo de cambio USD.");
            return null;
        }
    }

    private async Task<SbsTasaCambioDto?> ObtenerEurDesdeHtmlAsync(CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("SbsHtmlClient");
            var html = await client.GetStringAsync(_urlHtml, ct);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Buscar la fecha publicada en la página (patrón dd/MM/yyyy)
            if (!TryExtraerFechaDeHtml(html, out var fecha))
            {
                _logger.LogWarning("[SBS-HTML] No se pudo extraer la fecha de la página EUR.");
                return null;
            }

            var filas = doc.DocumentNode.SelectNodes("//tr");
            if (filas is null)
            {
                _logger.LogWarning("[SBS-HTML] No se encontraron filas de tabla para EUR.");
                return null;
            }

            foreach (var fila in filas)
            {
                var celdas = fila.SelectNodes("td");
                if (celdas is null || celdas.Count < 3) continue;

                var nombreMoneda = celdas[0].InnerText.Trim();
                if (!nombreMoneda.Contains("Euro", StringComparison.OrdinalIgnoreCase)) continue;

                var compraStr = celdas[1].InnerText.Trim();
                var ventaStr  = celdas[2].InnerText.Trim();

                if (string.IsNullOrWhiteSpace(compraStr) || string.IsNullOrWhiteSpace(ventaStr))
                    continue;

                _logger.LogDebug("[SBS-HTML] Fecha SBS: {Fecha} | Compra: {Compra} | Venta: {Venta}", fecha, compraStr, ventaStr);

                return new SbsTasaCambioDto
                {
                    CodigoMoneda      = "EUR",
                    DescripcionMoneda = "Euro",
                    Fecha             = fecha,
                    ValorCompra       = NormalizarDecimal(compraStr),
                    ValorVenta        = NormalizarDecimal(ventaStr)
                };
            }

            _logger.LogWarning("[SBS-HTML] No se encontró la fila de EUR en la página.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SBS-HTML] Error al obtener tipo de cambio EUR.");
            return null;
        }
    }

    // Busca el primer patrón dd/MM/yyyy en el HTML de la página SBS
    private static bool TryExtraerFechaDeHtml(string html, out DateOnly fecha)
    {
        var match = Regex.Match(html, @"\b(\d{2}/\d{2}/\d{4})\b");
        if (match.Success)
            return TryParseFechaSbs(match.Groups[1].Value, out fecha);

        fecha = default;
        return false;
    }

    private static bool TryParseFechaSbs(string? valor, out DateOnly fecha)
    {
        if (!string.IsNullOrWhiteSpace(valor) &&
            DateOnly.TryParseExact(valor.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
        {
            return true;
        }
        fecha = default;
        return false;
    }

    private static string NormalizarDecimal(string valor)
    {
        if (valor.Contains(',') && !valor.Contains('.'))
            return valor.Replace(',', '.');
        return valor;
    }
}
