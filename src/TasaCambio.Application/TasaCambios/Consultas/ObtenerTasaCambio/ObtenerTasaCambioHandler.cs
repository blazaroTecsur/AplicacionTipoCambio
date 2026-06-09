using Mapster;
using MediatR;
using TasaCambio.Application.Comun.Dtos;
using TasaCambio.Domain.Excepciones;
using TasaCambio.Domain.Interfaces;

namespace TasaCambio.Application.TasaCambios.Consultas.ObtenerTasaCambio;

internal sealed class ObtenerTasaCambioHandler : IRequestHandler<ObtenerTasaCambioQuery, ResponseDto<TasaCambioDto>>
{
    private readonly IUnidadDeTrabajo _uow;

    public ObtenerTasaCambioHandler(IUnidadDeTrabajo uow) => _uow = uow;

    public async Task<ResponseDto<TasaCambioDto>> Handle(ObtenerTasaCambioQuery request, CancellationToken ct)
    {
        var tasa = await _uow.TasaCambios.ObtenerPorFechaAsync(request.Empresa, request.CodigoMoneda, request.Fecha, ct)
            ?? throw new NotFoundException(nameof(Domain.Entidades.TasaCambio), $"{request.Empresa}/{request.CodigoMoneda}/{request.Fecha}");

        return ResponseDto<TasaCambioDto>.Ok(tasa.Adapt<TasaCambioDto>());
    }
}
