using Mapster;
using MediatR;
using TasaCambio.Aplicacion.Comun.Dtos;
using TasaCambio.Aplicacion.Comun.Interfaces;
using TasaCambio.Dominio.Interfaces;

namespace TasaCambio.Aplicacion.TasaCambios.Comandos.CrearTasaCambio;

internal sealed class CrearTasaCambioHandler : IRequestHandler<CrearTasaCambioCommand, RespuestaDto<TasaCambioDto>>
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

    public async Task<RespuestaDto<TasaCambioDto>> Handle(CrearTasaCambioCommand request, CancellationToken ct)
    {
        var tasa = Dominio.Entidades.TasaCambio.Crear(
            request.Empresa,
            request.CodigoMoneda,
            request.Fecha,
            request.ValorCompra,
            request.ValorVenta,
            _contextoUsuario.NombreUsuario,
            request.FuenteOrigen);

        await _uow.TasaCambios.AgregarAsync(tasa, ct);
        await _uow.GuardarCambiosAsync(ct);
        await _auditoria.RegistrarAsync("CREAR", nameof(Dominio.Entidades.TasaCambio), new { tasa.Empresa, tasa.CodigoMoneda, tasa.Fecha }, ct);

        return RespuestaDto<TasaCambioDto>.Ok(tasa.Adapt<TasaCambioDto>(), "Tasa de cambio creada correctamente.");
    }
}
