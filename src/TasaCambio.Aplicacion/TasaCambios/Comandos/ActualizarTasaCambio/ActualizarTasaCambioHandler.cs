using Mapster;
using MediatR;
using TasaCambio.Aplicacion.Comun.Dtos;
using TasaCambio.Aplicacion.Comun.Interfaces;
using TasaCambio.Dominio.Excepciones;
using TasaCambio.Dominio.Interfaces;

namespace TasaCambio.Aplicacion.TasaCambios.Comandos.ActualizarTasaCambio;

internal sealed class ActualizarTasaCambioHandler : IRequestHandler<ActualizarTasaCambioCommand, RespuestaDto<TasaCambioDto>>
{
    private readonly IUnidadDeTrabajo _uow;
    private readonly IContextoUsuario _contextoUsuario;
    private readonly IServicioAuditoria _auditoria;

    public ActualizarTasaCambioHandler(IUnidadDeTrabajo uow, IContextoUsuario contextoUsuario, IServicioAuditoria auditoria)
    {
        _uow = uow;
        _contextoUsuario = contextoUsuario;
        _auditoria = auditoria;
    }

    public async Task<RespuestaDto<TasaCambioDto>> Handle(ActualizarTasaCambioCommand request, CancellationToken ct)
    {
        var tasa = await _uow.TasaCambios.ObtenerPorIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Dominio.Entidades.TasaCambio), request.Id);

        tasa.ActualizarValores(request.ValorCompra, request.ValorVenta, _contextoUsuario.NombreUsuario);
        await _uow.TasaCambios.ActualizarAsync(tasa, ct);
        await _uow.GuardarCambiosAsync(ct);
        await _auditoria.RegistrarAsync("ACTUALIZAR", nameof(Dominio.Entidades.TasaCambio), new { request.Id }, ct);

        return RespuestaDto<TasaCambioDto>.Ok(tasa.Adapt<TasaCambioDto>(), "Tasa de cambio actualizada correctamente.");
    }
}
