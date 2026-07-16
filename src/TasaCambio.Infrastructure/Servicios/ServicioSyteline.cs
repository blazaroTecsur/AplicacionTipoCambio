using Infor.Abstractions.DTOs;
using Infor.Abstractions.Interfaces;
using Infor.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TasaCambio.Application.Comun.Interfaces;

namespace TasaCambio.Infrastructure.Servicios;

internal sealed class ServicioSyteline : IServicioSyteline
{
    private readonly IInforIdoService _idoService;
    private readonly InforSettings    _settings;
    private readonly ILogger<ServicioSyteline> _logger;

    private const string IDO = "SLCurrates";

    public ServicioSyteline(
        IInforIdoService idoService,
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
            // Formato ISO que acepta el filtro IDO (igual al ejemplo del usuario)
            var fechaIdo = fecha.ToString("yyyy-MM-dd");
            var itemId   = await BuscarItemIdAsync(codigoMoneda, fechaIdo, ct);

            var propiedades = BuildPropiedades(codigoMoneda, fechaIdo, compra, venta, usuario, _settings.MonedaBase, incluirClave: itemId is null);

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

    private async Task<string?> BuscarItemIdAsync(string codigoMoneda, string fechaIdo, CancellationToken ct)
    {
        // ToCurrCode = PEN (siempre), FromCurrCode = USD/EUR
        var filter = $"ToCurrCode='{_settings.MonedaBase}' AND FromCurrCode='{codigoMoneda}' AND EffDate='{fechaIdo}'";

        var result = await _idoService.LoadAsync(
            IDO,
            properties: "_ItemId",
            filter:     filter,
            recordCap:  1,
            orderBy:    "EffDate DESC",
            ct:         ct);

        if (result.Items.Count == 0)
            return null;

        return result.Items[0]
            .FirstOrDefault(p => p.Name == "_ItemId")
            ?.Value;
    }

    private static IEnumerable<IdoProperty> BuildPropiedades(
        string codigoMoneda, string fechaIdo, decimal compra, decimal venta,
        string usuario, string monedaBase, bool incluirClave)
    {
        var props = new List<IdoProperty>();

        if (incluirClave)
        {
            props.Add(new IdoProperty { Name = "ToCurrCode",   Value = monedaBase   }); // PEN
            props.Add(new IdoProperty { Name = "FromCurrCode", Value = codigoMoneda }); // USD / EUR
            props.Add(new IdoProperty { Name = "EffDate",      Value = fechaIdo     });
        }

        props.Add(new IdoProperty { Name = "BuyRate",  Value = compra.ToString("F4", System.Globalization.CultureInfo.InvariantCulture) });
        props.Add(new IdoProperty { Name = "SellRate", Value = venta.ToString("F4",  System.Globalization.CultureInfo.InvariantCulture) });
        props.Add(new IdoProperty { Name = "UserCode", Value = usuario });

        return props;
    }
}
