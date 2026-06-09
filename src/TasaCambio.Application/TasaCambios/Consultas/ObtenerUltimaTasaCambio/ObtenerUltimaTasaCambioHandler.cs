using Mapster;
using MediatR;
using TasaCambio.Application.Comun.Dtos;
using TasaCambio.Domain.Excepciones;
using TasaCambio.Domain.Interfaces;

namespace TasaCambio.Application.TasaCambios.Consultas.ObtenerUltimaTasaCambio;

internal sealed class ObtenerUltimaTasaCambioHandler : IRequestHandler<ObtenerUltimaTasaCambioQuery, ResponseDto<TasaCambioDto>>
{
    private readonly IUnidadDeTrabajo _uow;

    public ObtenerUltimaTasaCambioHandler(IUnidadDeTrabajo uow) => _uow = uow;

    public async Task<ResponseDto<TasaCambioDto>> Handle(ObtenerUltimaTasaCambioQuery request, CancellationToken ct)
    {
        var tasa = await _uow.TasaCambios.ObtenerUltimaHastaFechaAsync(request.Empresa, request.CodigoMoneda, request.HastaFecha, ct)
            ?? throw new NotFoundException(nameof(Dominio.Entidades.TasaCambio), $"{request.Empresa}/{request.CodigoMoneda}/hasta-{request.HastaFecha}");

        return ResponseDto<TasaCambioDto>.Ok(tasa.Adapt<TasaCambioDto>());
    }
}
