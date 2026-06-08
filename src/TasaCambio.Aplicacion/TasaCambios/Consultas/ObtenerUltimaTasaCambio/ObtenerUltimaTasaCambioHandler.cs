using Mapster;
using MediatR;
using TasaCambio.Aplicacion.Comun.Dtos;
using TasaCambio.Dominio.Excepciones;
using TasaCambio.Dominio.Interfaces;

namespace TasaCambio.Aplicacion.TasaCambios.Consultas.ObtenerUltimaTasaCambio;

internal sealed class ObtenerUltimaTasaCambioHandler : IRequestHandler<ObtenerUltimaTasaCambioQuery, RespuestaDto<TasaCambioDto>>
{
    private readonly IUnidadDeTrabajo _uow;

    public ObtenerUltimaTasaCambioHandler(IUnidadDeTrabajo uow) => _uow = uow;

    public async Task<RespuestaDto<TasaCambioDto>> Handle(ObtenerUltimaTasaCambioQuery request, CancellationToken ct)
    {
        var tasa = await _uow.TasaCambios.ObtenerUltimaHastaFechaAsync(request.Empresa, request.CodigoMoneda, request.HastaFecha, ct)
            ?? throw new NotFoundException(nameof(Dominio.Entidades.TasaCambio), $"{request.Empresa}/{request.CodigoMoneda}/hasta-{request.HastaFecha}");

        return RespuestaDto<TasaCambioDto>.Ok(tasa.Adapt<TasaCambioDto>());
    }
}
