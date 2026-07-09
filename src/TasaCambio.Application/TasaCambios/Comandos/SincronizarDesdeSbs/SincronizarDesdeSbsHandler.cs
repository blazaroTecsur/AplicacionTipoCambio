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

        var valorCompra = decimal.Parse(sbsDto.ValorCompra, System.Globalization.CultureInfo.InvariantCulture);
        var valorVenta  = decimal.Parse(sbsDto.ValorVenta,  System.Globalization.CultureInfo.InvariantCulture);
        var usuario     = _contextoUsuario.NombreUsuario;

        // 2. Guardar en BD interna (contingencia)
        var tasaExistente = await _uow.TasaCambios.ObtenerPorFechaAsync(request.CodigoMoneda, request.Fecha, ct);

        TasaCambioDto tasaDto;
        string mensajeDb;

        if (tasaExistente is not null)
        {
            if (tasaExistente.ValorCompra == valorCompra && tasaExistente.ValorVenta == valorVenta)
            {
                tasaDto    = tasaExistente.Adapt<TasaCambioDto>();
                mensajeDb  = "La tasa de cambio ya estaba actualizada en la BD interna.";
            }
            else
            {
                tasaExistente.ActualizarValores(valorCompra, valorVenta, usuario, "SBS");
                await _uow.TasaCambios.ActualizarAsync(tasaExistente, ct);
                await _uow.GuardarCambiosAsync(ct);
                await _auditoria.RegistrarAsync("ACTUALIZAR_SBS", nameof(Domain.Entidades.TasaCambio),
                    new { tasaExistente.CodigoMoneda, tasaExistente.Fecha }, ct);

                tasaDto   = tasaExistente.Adapt<TasaCambioDto>();
                mensajeDb = "Tasa actualizada en BD interna.";
            }
        }
        else
        {
            var tasa = Domain.Entidades.TasaCambio.Crear(
                request.CodigoMoneda, request.Fecha, valorCompra, valorVenta, usuario, "SBS");

            await _uow.TasaCambios.AgregarAsync(tasa, ct);
            await _uow.GuardarCambiosAsync(ct);
            await _auditoria.RegistrarAsync("SINCRONIZAR_SBS", nameof(Domain.Entidades.TasaCambio),
                new { tasa.CodigoMoneda, tasa.Fecha }, ct);

            tasaDto   = tasa.Adapt<TasaCambioDto>();
            mensajeDb = "Tasa registrada en BD interna.";
        }

        // 3. Registrar en SyteLine (mejor esfuerzo — no falla la operación si SyteLine no responde)
        try
        {
            var sincronizado = await _servicioSyteline.RegistrarTasaCambioAsync(
                request.CodigoMoneda, request.Fecha, valorCompra, valorVenta, usuario, ct);

            if (!sincronizado)
                _logger.LogWarning("[SYTELINE] No se pudo registrar {Moneda}/{Fecha} en SyteLine.", request.CodigoMoneda, request.Fecha);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SYTELINE] Error al intentar registrar {Moneda}/{Fecha} en SyteLine.", request.CodigoMoneda, request.Fecha);
        }

        return ResponseDto<TasaCambioDto>.Ok(tasaDto, mensajeDb);
    }
}
