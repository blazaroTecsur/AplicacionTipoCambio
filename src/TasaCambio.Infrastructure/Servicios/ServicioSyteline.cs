using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TasaCambio.Application.Comun.Interfaces;
using TasaCambio.Infrastructure.Infor;

namespace TasaCambio.Infrastructure.Servicios;

internal sealed class ServicioSyteline : IServicioSyteline
{
    private readonly InforIdoService _idoService;
    private readonly InforSettings   _settings;
    private readonly ILogger<ServicioSyteline> _logger;

    private const string IDO = "SLCurrates";

    public ServicioSyteline(
        InforIdoService idoService,
        IOptions<InforSettings> settings,
        ILogger<ServicioSyteline> logger)
    {
        _idoService = idoService;
        _settings   = settings.Value;
        _logger     = logger;
    }

    public async Task<bool> RegistrarTasaCambioAsync(
        string codigoMoneda, DateOnly fecha, decimal compra, decimal venta,
        string usuario, CancellationToken ct = default)
    {
        try
        {
            var fechaIdo  = fecha.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-ddTHH:mm:ss");
            var itemId    = await BuscarItemIdAsync(codigoMoneda, fechaIdo, ct);

            var propiedades = BuildPropiedades(codigoMoneda, fechaIdo, compra, venta, usuario, incluirClave: itemId is null);

            if (itemId is null)
                await _idoService.InsertItemAsync(IDO, propiedades, ct: ct);
            else
                await _idoService.UpdateItemAsync(IDO, itemId, propiedades, ct: ct);

            _logger.LogInformation(
                "[SYTELINE-IDO] {Accion} {Moneda} — Compra: {Compra} / Venta: {Venta} / Fecha: {Fecha}",
                itemId is null ? "INSERT" : "UPDATE", codigoMoneda, compra, venta, fecha);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SYTELINE-IDO] Error al registrar {Moneda} para {Fecha}", codigoMoneda, fecha);
            return false;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<string?> BuscarItemIdAsync(string codigoMoneda, string fechaIdo, CancellationToken ct)
    {
        var filter = $"ToCurrCode='{codigoMoneda}' AND FromCurrCode='{_settings.MonedaBase}' AND EffDate='{fechaIdo}'";
        var result = await _idoService.LoadAsync(IDO, props: "ItemId", filter: filter, recordCap: 1, ct: ct);

        if (result.ValueKind == JsonValueKind.Undefined)
            return null;

        // Estructura de respuesta IDO: { "Items": [ [ { "Name": "ItemId", "Value": "xxx" } ] ] }
        if (result.TryGetProperty("Items", out var items) &&
            items.GetArrayLength() > 0)
        {
            var primeraFila = items[0];
            foreach (var prop in primeraFila.EnumerateArray())
            {
                if (prop.TryGetProperty("Name",  out var nombre) &&
                    prop.TryGetProperty("Value", out var valor)  &&
                    nombre.GetString() == "ItemId")
                {
                    return valor.GetString();
                }
            }
        }

        return null;
    }

    private IEnumerable<InforIdoService.IdoPropiedad> BuildPropiedades(
        string codigoMoneda, string fechaIdo, decimal compra, decimal venta,
        string usuario, bool incluirClave)
    {
        var props = new List<InforIdoService.IdoPropiedad>();

        if (incluirClave)
        {
            props.Add(new("ToCurrCode",   codigoMoneda));
            props.Add(new("FromCurrCode", _settings.MonedaBase));
            props.Add(new("EffDate",      fechaIdo));
        }

        props.Add(new("BuyRate",   compra.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)));
        props.Add(new("SellRate",  venta.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)));
        props.Add(new("UserCode",  usuario));

        return props;
    }
}
