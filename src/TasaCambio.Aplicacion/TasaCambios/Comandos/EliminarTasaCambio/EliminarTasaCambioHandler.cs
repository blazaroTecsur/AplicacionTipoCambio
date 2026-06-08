using MediatR;
using TasaCambio.Aplicacion.Comun.Dtos;
using TasaCambio.Aplicacion.Comun.Interfaces;
using TasaCambio.Dominio.Excepciones;
using TasaCambio.Dominio.Interfaces;

namespace TasaCambio.Aplicacion.TasaCambios.Comandos.EliminarTasaCambio;

internal sealed class EliminarTasaCambioHandler : IRequestHandler<EliminarTasaCambioCommand, RespuestaDto<bool>>
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

    public async Task<RespuestaDto<bool>> Handle(EliminarTasaCambioCommand request, CancellationToken ct)
    {
        var tasa = await _uow.TasaCambios.ObtenerPorIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Dominio.Entidades.TasaCambio), request.Id);

        await _uow.TasaCambios.EliminarAsync(tasa, ct);
        await _uow.GuardarCambiosAsync(ct);
        await _auditoria.RegistrarAsync("ELIMINAR", nameof(Dominio.Entidades.TasaCambio), new { request.Id, _contextoUsuario.NombreUsuario }, ct);

        return RespuestaDto<bool>.Ok(true, "Tasa de cambio eliminada correctamente.");
    }
}
