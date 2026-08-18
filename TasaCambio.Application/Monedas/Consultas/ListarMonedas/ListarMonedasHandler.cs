using Mapster;
using MediatR;
using TasaCambio.Application.Comun.Dtos;
using TasaCambio.Domain.Interfaces;

namespace TasaCambio.Application.Monedas.Consultas.ListarMonedas;

internal sealed class ListarMonedasHandler : IRequestHandler<ListarMonedasQuery, ResponseDto<IReadOnlyList<MonedaDto>>>
{
    private readonly IUnidadDeTrabajo _uow;

    public ListarMonedasHandler(IUnidadDeTrabajo uow) => _uow = uow;

    public async Task<ResponseDto<IReadOnlyList<MonedaDto>>> Handle(ListarMonedasQuery request, CancellationToken ct)
    {
        var monedas = await _uow.Monedas.ListarTodasAsync(ct);
        return ResponseDto<IReadOnlyList<MonedaDto>>.Ok(monedas.Adapt<IReadOnlyList<MonedaDto>>());
    }
}
