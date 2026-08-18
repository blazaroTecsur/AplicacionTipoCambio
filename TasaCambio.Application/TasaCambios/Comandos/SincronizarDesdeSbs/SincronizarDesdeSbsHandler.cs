using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using TasaCambio.Application.Comun.Dtos;
using TasaCambio.Application.Comun.Interfaces;
using TasaCambio.Domain.Excepciones;
using TasaCambio.Domain.Interfaces;

namespace TasaCambio.Application.TasaCambios.Comandos.SincronizarDesdeSbs;

internal sealed class SincronizarDesdeSbsHandler : IRequestHandler<SincronizarDesdeSbsCommand, ResponseDto<TasaCambioDto>>
{
    private readonly IServicioSbs _servicioSbs;
    private readonly IServicioSyteline _servicioSyteline;
    private readonly IUnidadDeTrabajo _uow;
    private readonly IContextoUsuario _contextoUsuario;
    private readonly IServicioAuditoria _auditoria;
    private readonly ILogger<SincronizarDesdeSbsHandler> _logger;

    public SincronizarDesdeSbsHandler(
        IServicioSbs servicioSbs,
        IServicioSyteline servicioSyteline,
        IUnidadDeTrabajo uow,
        IContextoUsuario contextoUsuario,
        IServicioAuditoria auditoria,
        ILogger<SincronizarDesdeSbsHandler> logger)
    {
        _servicioSbs = servicioSbs;
        _servicioSyteline = servicioSyteline;
        _uow = uow;
        _contextoUsuario = contextoUsuario;
        _auditoria = auditoria;
        _logger = logger;
    }

    public async Task<ResponseDto<TasaCambioDto>> Handle(SincronizarDesdeSbsCommand request, CancellationToken ct)
    {
        // 1. Obtener tipo de cambio desde SBS
        var sbsDto = await _servicioSbs.ObtenerTasaCambioAsync(request.CodigoMoneda, request.Fecha, ct);

        if (sbsDto is null)
            throw new NotFoundException("TasaCambioSbs", $"{request.CodigoMoneda}/{request.Fecha:ddMMyyyy}");

        // La fecha autoritativa es la que publica SBS (puede diferir de la fecha del request)
        var fechaSbs    = sbsDto.Fecha;
        var valorCompra = decimal.Parse(sbsDto.ValorCompra, System.Globalization.CultureInfo.InvariantCulture);
        var valorVenta  = decimal.Parse(sbsDto.ValorVenta,  System.Globalization.CultureInfo.InvariantCulture);
        var usuario     = _contextoUsuario.NombreUsuario;

        // 2. Guardar en BD interna (contingencia)
        var tasaExistente = await _uow.TasaCambios.ObtenerPorFechaAsync(request.CodigoMoneda, fechaSbs, ct);

        Domain.Entidades.TasaCambio entidad;
        string mensajeDb;
        bool valoresNoCambiaron;

        if (tasaExistente is not null)
        {
            if (tasaExistente.ValorCompra == valorCompra && tasaExistente.ValorVenta == valorVenta)
            {
                entidad            = tasaExistente;
                mensajeDb          = "La tasa de cambio ya estaba actualizada en la BD interna.";
                valoresNoCambiaron = true;

                _logger.LogInformation(
                    "[BD] Sin cambios {Moneda}/{Fecha} — Compra: {Compra} / Venta: {Venta}",
                    request.CodigoMoneda, fechaSbs, valorCompra, valorVenta);
            }
            else
            {
                tasaExistente.ActualizarValores(valorCompra, valorVenta, usuario, "SBS");
                await _uow.TasaCambios.ActualizarAsync(tasaExistente, ct);
                await _uow.GuardarCambiosAsync(ct);
                await _auditoria.RegistrarAsync("ACTUALIZAR_SBS", nameof(Domain.Entidades.TasaCambio),
                    new { tasaExistente.CodigoMoneda, tasaExistente.Fecha }, ct);

                entidad            = tasaExistente;
                mensajeDb          = "Tasa actualizada en BD interna.";
                valoresNoCambiaron = false;

                _logger.LogInformation(
                    "[BD] Actualizado {Moneda}/{Fecha} — Compra: {Compra} / Venta: {Venta}",
                    request.CodigoMoneda, fechaSbs, valorCompra, valorVenta);
            }
        }
        else
        {
            var tasa = Domain.Entidades.TasaCambio.Crear(
                request.CodigoMoneda, fechaSbs, valorCompra, valorVenta, usuario, "SBS");

            await _uow.TasaCambios.AgregarAsync(tasa, ct);
            await _uow.GuardarCambiosAsync(ct);
            await _auditoria.RegistrarAsync("SINCRONIZAR_SBS", nameof(Domain.Entidades.TasaCambio),
                new { tasa.CodigoMoneda, tasa.Fecha }, ct);

            entidad            = tasa;
            mensajeDb          = "Tasa registrada en BD interna.";
            valoresNoCambiaron = false;

            _logger.LogInformation(
                "[BD] Registrado {Moneda}/{Fecha} — Compra: {Compra} / Venta: {Venta}",
                request.CodigoMoneda, fechaSbs, valorCompra, valorVenta);
        }

        var tasaDto       = entidad.Adapt<TasaCambioDto>();
        bool estadoSyncPrevio = entidad.SincronizadoSyteline;

        // 3. Registrar en SyteLine (mejor esfuerzo — no falla la operación si SyteLine no responde)
        // Optimización: si los valores no cambiaron y SyteLine ya estaba sincronizado, no hay nada que enviar
        bool sincronizado = estadoSyncPrevio;
        if (valoresNoCambiaron && estadoSyncPrevio)
        {
            _logger.LogInformation(
                "[SYTELINE] Omitido {Moneda}/{Fecha} — sin cambios y ya sincronizado.",
                request.CodigoMoneda, fechaSbs);
        }
        else
        {
            try
            {
                sincronizado = await _servicioSyteline.RegistrarTasaCambioAsync(
                    request.CodigoMoneda, fechaSbs, valorCompra, valorVenta, usuario, ct);

                if (!sincronizado)
                    _logger.LogWarning(
                        "[SYTELINE] No se pudo registrar {Moneda}/{Fecha}. Último éxito: {UltimoExito}.",
                        request.CodigoMoneda, fechaSbs,
                        entidad.FechaUltSincSyteline.HasValue
                            ? entidad.FechaUltSincSyteline.Value.ToString("dd/MM/yyyy HH:mm") + " UTC"
                            : "nunca");
                else
                    _logger.LogInformation(
                        "[SYTELINE] Registrado {Moneda}/{Fecha} — Compra: {Compra} / Venta: {Venta}",
                        request.CodigoMoneda, fechaSbs, valorCompra, valorVenta);
            }
            catch (Exception ex)
            {
                sincronizado = false;
                _logger.LogError(ex,
                    "[SYTELINE] Error al registrar {Moneda}/{Fecha}. Último éxito: {UltimoExito}.",
                    request.CodigoMoneda, fechaSbs,
                    entidad.FechaUltSincSyteline.HasValue
                        ? entidad.FechaUltSincSyteline.Value.ToString("dd/MM/yyyy HH:mm") + " UTC"
                        : "nunca");
            }

            // Persiste el cambio de estado de sincronización solo cuando varía
            entidad.MarcarSincronizadoSyteline(sincronizado);
            if (entidad.SincronizadoSyteline != estadoSyncPrevio)
            {
                await _uow.TasaCambios.ActualizarAsync(entidad, ct);
                await _uow.GuardarCambiosAsync(ct);
            }
        }

        return ResponseDto<TasaCambioDto>.Ok(tasaDto, mensajeDb);
    }
}
