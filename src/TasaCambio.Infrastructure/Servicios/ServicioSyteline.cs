using Infor.Abstractions.DTOs;
using Infor.Abstractions.Interfaces;
using Infor.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TasaCambio.Application.Comun.Interfaces;

namespace TasaCambio.Infrastructure.Servicios;

internal sealed class ServicioSyteline : IServicioSyteline
{
    private readonly IInforIdoService _idoService;
    private readonly ILogger<ServicioSyteline> _logger;

    private const string IDO        = "SLCurrates";
    private const string MonedaBase = "PEN";

    public ServicioSyteline(
        IInforIdoService idoService,
        ILogger<ServicioSyteline> logger)
    {
        _idoService = idoService;
        _logger     = logger;
    }

    public async Task<bool> RegistrarTasaCambioAsync(
        string codigoMoneda, DateOnly fecha, decimal compra, decimal venta,
        string usuario, CancellationToken ct = default)
    {
        try
        {
            var fechaIdo    = fecha.ToString("yyyy-MM-dd");
            var itemId      = await BuscarItemIdAsync(codigoMoneda, fechaIdo, ct);
            var propiedades = BuildPropiedades(codigoMoneda, fechaIdo, compra, venta, usuario, incluirClave: itemId is null);

            if (itemId is null)
            {
                await _idoService.InsertItemAsync(IDO, propiedades, ct: ct);
            }
            else
            {
                var payload = new IdoUpdatePayload(IDO,
                    [new IdoUpdateChange(Action: 2, ItemId: itemId, RefreshAfterSave: false, Properties: propiedades)]);
                await _idoService.UpdateItemAsync(IDO, payload, ct);
            }

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
        var filter = $"ToCurrCode='{MonedaBase}' AND FromCurrCode='{codigoMoneda}' AND EffDate='{fechaIdo}'";

        var result = await _idoService.LoadAsync(
            IDO,
            props: "_ItemId",
            filter:     filter,
            recordCap:  1,
            orderBy:    "EffDate DESC",
            ct:         ct);

        if (!result.TryGetProperty("Items", out var items) || items.GetArrayLength() == 0)
            return null;

        var first = items[0];
        return first.TryGetProperty("_ItemId", out var itemIdProp) ? itemIdProp.GetString() : null;
    }

    private static List<IdoProperty> BuildPropiedades(
        string codigoMoneda, string fechaIdo, decimal compra, decimal venta,
        string usuario, bool incluirClave)
    {
        var props = new List<IdoProperty>();

        if (incluirClave)
        {
            props.Add(new IdoProperty { Name = "ToCurrCode",   Value = MonedaBase   });
            props.Add(new IdoProperty { Name = "FromCurrCode", Value = codigoMoneda });
            props.Add(new IdoProperty { Name = "EffDate",      Value = fechaIdo     });
        }

        props.Add(new IdoProperty { Name = "BuyRate",  Value = compra.ToString("F4", System.Globalization.CultureInfo.InvariantCulture) });
        props.Add(new IdoProperty { Name = "SellRate", Value = venta.ToString("F4",  System.Globalization.CultureInfo.InvariantCulture) });
        props.Add(new IdoProperty { Name = "UserCode", Value = usuario });

        return props;
    }
}

internal sealed record IdoUpdatePayload(string IDOName, IdoUpdateChange[] Changes);
internal sealed record IdoUpdateChange(int Action, string? ItemId, bool RefreshAfterSave, List<IdoProperty> Properties);
