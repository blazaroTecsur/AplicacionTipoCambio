using Mapster;
using MediatR;
using TasaCambio.Application.Comun.Dtos;
using TasaCambio.Application.Comun.Interfaces;
using TasaCambio.Domain.Excepciones;
using TasaCambio.Domain.Interfaces;

namespace TasaCambio.Application.TasaCambios.Comandos.SincronizarDesdeSbs;

internal sealed class SincronizarDesdeSbsHandler : IRequestHandler<SincronizarDesdeSbsCommand, ResponseDto<TasaCambioDto>>
{
    private readonly IServicioSbs _servicioSbs;
    private readonly IUnidadDeTrabajo _uow;
    private readonly IContextoUsuario _contextoUsuario;
    private readonly IServicioAuditoria _auditoria;

    public SincronizarDesdeSbsHandler(
        IServicioSbs servicioSbs,
        IUnidadDeTrabajo uow,
        IContextoUsuario contextoUsuario,
        IServicioAuditoria auditoria)
    {
        _servicioSbs = servicioSbs;
        _uow = uow;
        _contextoUsuario = contextoUsuario;
        _auditoria = auditoria;
    }

    public async Task<ResponseDto<TasaCambioDto>> Handle(SincronizarDesdeSbsCommand request, CancellationToken ct)
    {
        var sbsDto = await _servicioSbs.ObtenerTasaCambioAsync(request.CodigoMoneda, request.Fecha, ct);

        if (sbsDto is null)
            throw new NotFoundException("TasaCambioSbs", $"{request.CodigoMoneda}/{request.Fecha:ddMMyyyy}");

        var valorCompra = decimal.Parse(sbsDto.ValorCompra, System.Globalization.CultureInfo.InvariantCulture);
        var valorVenta = decimal.Parse(sbsDto.ValorVenta, System.Globalization.CultureInfo.InvariantCulture);

        var tasaExistente = await _uow.TasaCambios.ObtenerPorFechaAsync(request.CodigoMoneda, request.Fecha, ct);

        if (tasaExistente is not null)
        {
            if (tasaExistente.ValorCompra == valorCompra && tasaExistente.ValorVenta == valorVenta)
                return ResponseDto<TasaCambioDto>.Ok(tasaExistente.Adapt<TasaCambioDto>(), "La tasa de cambio ya estaba actualizada.");

            tasaExistente.ActualizarValores(valorCompra, valorVenta, _contextoUsuario.NombreUsuario, "SBS");
            await _uow.TasaCambios.ActualizarAsync(tasaExistente, ct);
            await _uow.GuardarCambiosAsync(ct);
            await _auditoria.RegistrarAsync("ACTUALIZAR_SBS", nameof(Domain.Entidades.TasaCambio), new { tasaExistente.CodigoMoneda, tasaExistente.Fecha }, ct);

            return ResponseDto<TasaCambioDto>.Ok(tasaExistente.Adapt<TasaCambioDto>(), "Tasa de cambio actualizada desde SBS correctamente.");
        }

        var tasa = Domain.Entidades.TasaCambio.Crear(
            request.CodigoMoneda,
            request.Fecha,
            valorCompra,
            valorVenta,
            _contextoUsuario.NombreUsuario,
            "SBS");

        await _uow.TasaCambios.AgregarAsync(tasa, ct);
        await _uow.GuardarCambiosAsync(ct);
        await _auditoria.RegistrarAsync("SINCRONIZAR_SBS", nameof(Domain.Entidades.TasaCambio), new { tasa.CodigoMoneda, tasa.Fecha }, ct);

        return ResponseDto<TasaCambioDto>.Ok(tasa.Adapt<TasaCambioDto>(), "Tasa de cambio sincronizada desde SBS correctamente.");
    }
}
