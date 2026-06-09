using MediatR;
using TasaCambio.Application.Comun.Dtos;
using TasaCambio.Application.Comun.Interfaces;
using TasaCambio.Domain.Excepciones;
using TasaCambio.Domain.Interfaces;

namespace TasaCambio.Application.TasaCambios.Comandos.EliminarTasaCambio;

internal sealed class EliminarTasaCambioHandler : IRequestHandler<EliminarTasaCambioCommand, ResponseDto<bool>>
{
    private readonly IUnidadDeTrabajo _uow;
    private readonly IContextoUsuario _contextoUsuario;
    private readonly IServicioAuditoria _auditoria;

    public EliminarTasaCambioHandler(IUnidadDeTrabajo uow, IContextoUsuario contextoUsuario, IServicioAuditoria auditoria)
    {
        _uow = uow;
        _contextoUsuario = contextoUsuario;
        _auditoria = auditoria;
    }

    public async Task<ResponseDto<bool>> Handle(EliminarTasaCambioCommand request, CancellationToken ct)
    {
        var tasa = await _uow.TasaCambios.ObtenerPorIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Dominio.Entidades.TasaCambio), request.Id);

        await _uow.TasaCambios.EliminarAsync(tasa, ct);
        await _uow.GuardarCambiosAsync(ct);
        await _auditoria.RegistrarAsync("ELIMINAR", nameof(Dominio.Entidades.TasaCambio), new { request.Id, _contextoUsuario.NombreUsuario }, ct);

        return ResponseDto<bool>.Ok(true, "Tasa de cambio eliminada correctamente.");
    }
}
