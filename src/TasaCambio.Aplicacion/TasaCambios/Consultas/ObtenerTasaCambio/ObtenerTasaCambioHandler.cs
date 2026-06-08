using Mapster;
using MediatR;
using TasaCambio.Aplicacion.Comun.Dtos;
using TasaCambio.Dominio.Excepciones;
using TasaCambio.Dominio.Interfaces;

namespace TasaCambio.Aplicacion.TasaCambios.Consultas.ObtenerTasaCambio;

internal sealed class ObtenerTasaCambioHandler : IRequestHandler<ObtenerTasaCambioQuery, RespuestaDto<TasaCambioDto>>
{
    private readonly IUnidadDeTrabajo _uow;

    public ObtenerTasaCambioHandler(IUnidadDeTrabajo uow) => _uow = uow;

    public async Task<RespuestaDto<TasaCambioDto>> Handle(ObtenerTasaCambioQuery request, CancellationToken ct)
    {
        var tasa = await _uow.TasaCambios.ObtenerPorFechaAsync(request.Empresa, request.CodigoMoneda, request.Fecha, ct)
            ?? throw new NotFoundException(nameof(Dominio.Entidades.TasaCambio), $"{request.Empresa}/{request.CodigoMoneda}/{request.Fecha}");

        return RespuestaDto<TasaCambioDto>.Ok(tasa.Adapt<TasaCambioDto>());
    }
}
