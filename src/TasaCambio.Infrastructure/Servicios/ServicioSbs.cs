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
            "USD" => ObtenerUsdDesdeXmlAsync(fecha, ct),
            "EUR" => ObtenerEurDesdeHtmlAsync(fecha, ct),
            _ => Task.FromResult<SbsTasaCambioDto?>(null)
        };
    }

    private async Task<SbsTasaCambioDto?> ObtenerUsdDesdeXmlAsync(DateOnly fecha, CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("SbsXmlClient");
            var xml = await client.GetStringAsync(_urlXml, ct);

            var doc = XDocument.Parse(xml);

            var compraStr = doc.Descendants()
                .FirstOrDefault(e => e.Name.LocalName.Equals("compra", StringComparison.OrdinalIgnoreCase))?.Value?.Trim();
            var ventaStr = doc.Descendants()
                .FirstOrDefault(e => e.Name.LocalName.Equals("venta", StringComparison.OrdinalIgnoreCase))?.Value?.Trim();

            if (string.IsNullOrWhiteSpace(compraStr) || string.IsNullOrWhiteSpace(ventaStr))
            {
                _logger.LogWarning("[SBS-XML] No se encontraron valores compra/venta en el XML para {Fecha}", fecha);
                return null;
            }

            return new SbsTasaCambioDto
            {
                CodigoMoneda = "USD",
                DescripcionMoneda = "Dólar Americano",
                ValorCompra = NormalizarDecimal(compraStr),
                ValorVenta = NormalizarDecimal(ventaStr)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SBS-XML] Error al obtener tipo de cambio USD para {Fecha}", fecha);
            return null;
        }
    }

    private async Task<SbsTasaCambioDto?> ObtenerEurDesdeHtmlAsync(DateOnly fecha, CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("SbsHtmlClient");
            var html = await client.GetStringAsync(_urlHtml, ct);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var filas = doc.DocumentNode.SelectNodes("//tr");
            if (filas is null)
            {
                _logger.LogWarning("[SBS-HTML] No se encontraron filas de tabla para {Fecha}", fecha);
                return null;
            }

            foreach (var fila in filas)
            {
                var celdas = fila.SelectNodes("td");
                if (celdas is null || celdas.Count < 3) continue;

                var nombreMoneda = celdas[0].InnerText.Trim();
                if (!nombreMoneda.Contains("Euro", StringComparison.OrdinalIgnoreCase)) continue;

                var compraStr = celdas[1].InnerText.Trim();
                var ventaStr = celdas[2].InnerText.Trim();

                if (string.IsNullOrWhiteSpace(compraStr) || string.IsNullOrWhiteSpace(ventaStr))
                    continue;

                return new SbsTasaCambioDto
                {
                    CodigoMoneda = "EUR",
                    DescripcionMoneda = "Euro",
                    ValorCompra = NormalizarDecimal(compraStr),
                    ValorVenta = NormalizarDecimal(ventaStr)
                };
            }

            _logger.LogWarning("[SBS-HTML] No se encontró la fila de EUR en la página para {Fecha}", fecha);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SBS-HTML] Error al obtener tipo de cambio EUR para {Fecha}", fecha);
            return null;
        }
    }

    // Normaliza el separador decimal a punto para que decimal.Parse con InvariantCulture funcione correctamente
    private static string NormalizarDecimal(string valor)
    {
        // Si contiene coma como separador decimal (ej: "3,655"), reemplaza por punto
        if (valor.Contains(',') && !valor.Contains('.'))
            return valor.Replace(',', '.');

        return valor;
    }
}
