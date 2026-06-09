using Mapster;
using MediatR;
using TasaCambio.Application.Comun.Dtos;
using TasaCambio.Domain.Interfaces;

namespace TasaCambio.Application.TasaCambios.Consultas.ListarTasasCambio;

internal sealed class ListarTasasCambioHandler : IRequestHandler<ListarTasasCambioQuery, ResponseDto<IReadOnlyList<TasaCambioDto>>>
{
    private readonly IUnidadDeTrabajo _uow;

    public ListarTasasCambioHandler(IUnidadDeTrabajo uow) => _uow = uow;

    public async Task<ResponseDto<IReadOnlyList<TasaCambioDto>>> Handle(ListarTasasCambioQuery request, CancellationToken ct)
    {
        var tasas = await _uow.TasaCambios.ListarPorEmpresaYMonedaAsync(
            request.Empresa, request.CodigoMoneda, request.Anio, request.Mes, ct);

        return ResponseDto<IReadOnlyList<TasaCambioDto>>.Ok(tasas.Adapt<IReadOnlyList<TasaCambioDto>>());
    }
}
