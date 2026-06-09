using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TasaCambio.Application.Comun.Interfaces;
using TasaCambio.Application.Sbs;

namespace TasaCambio.Infrastructure.Servicios;

internal sealed class ServicioSbs : IServicioSbs
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _baseUrl;
    private readonly string _token;

    public ServicioSbs(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _baseUrl = config["Sbs:Url"] ?? throw new InvalidOperationException("Configuración 'Sbs:Url' no encontrada.");
        _token = config["Sbs:Token"] ?? throw new InvalidOperationException("Configuración 'Sbs:Token' no encontrada.");
    }

    public async Task<SbsTasaCambioDto?> ObtenerTasaCambioAsync(string codigoMoneda, DateOnly fecha, CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("SbsClient");

            var fechaFormateada = fecha.ToString("ddMMyyyy");
            var url = $"{_baseUrl.TrimEnd('/')}/{fechaFormateada}/{codigoMoneda}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            using var response = await client.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
                return null;

            var contenido = await response.Content.ReadAsStringAsync(ct);

            if (string.IsNullOrWhiteSpace(contenido))
                return null;

            return JsonSerializer.Deserialize<SbsTasaCambioDto>(contenido);
        }
        catch
        {
            return null;
        }
    }
}
