using Mapster;
using MediatR;
using TasaCambio.Application.Comun.Dtos;
using TasaCambio.Application.Comun.Interfaces;
using TasaCambio.Domain.Interfaces;

namespace TasaCambio.Application.TasaCambios.Comandos.CrearTasaCambio;

internal sealed class CrearTasaCambioHandler : IRequestHandler<CrearTasaCambioCommand, ResponseDto<TasaCambioDto>>
{
    private readonly IUnidadDeTrabajo _uow;
    private readonly IContextoUsuario _contextoUsuario;
    private readonly IServicioAuditoria _auditoria;

    public CrearTasaCambioHandler(IUnidadDeTrabajo uow, IContextoUsuario contextoUsuario, IServicioAuditoria auditoria)
    {
        _uow = uow;
        _contextoUsuario = contextoUsuario;
        _auditoria = auditoria;
    }

    public async Task<ResponseDto<TasaCambioDto>> Handle(CrearTasaCambioCommand request, CancellationToken ct)
    {
        var tasa = Domain.Entidades.TasaCambio.Crear(
            request.Empresa,
            request.CodigoMoneda,
            request.Fecha,
            request.ValorCompra,
            request.ValorVenta,
            _contextoUsuario.NombreUsuario,
            request.FuenteOrigen);

        await _uow.TasaCambios.AgregarAsync(tasa, ct);
        await _uow.GuardarCambiosAsync(ct);
        await _auditoria.RegistrarAsync("CREAR", nameof(Domain.Entidades.TasaCambio), new { tasa.Empresa, tasa.CodigoMoneda, tasa.Fecha }, ct);

        return ResponseDto<TasaCambioDto>.Ok(tasa.Adapt<TasaCambioDto>(), "Tasa de cambio creada correctamente.");
    }
}
