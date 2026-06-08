using Mapster;
using MediatR;
using TasaCambio.Aplicacion.Comun.Dtos;
using TasaCambio.Dominio.Interfaces;

namespace TasaCambio.Aplicacion.Monedas.Consultas.ListarMonedas;

internal sealed class ListarMonedasHandler : IRequestHandler<ListarMonedasQuery, RespuestaDto<IReadOnlyList<MonedaDto>>>
{
    private readonly IUnidadDeTrabajo _uow;

    public ListarMonedasHandler(IUnidadDeTrabajo uow) => _uow = uow;

    public async Task<RespuestaDto<IReadOnlyList<MonedaDto>>> Handle(ListarMonedasQuery request, CancellationToken ct)
    {
        var monedas = await _uow.Monedas.ListarPorEmpresaAsync(request.Empresa, ct);
        return RespuestaDto<IReadOnlyList<MonedaDto>>.Ok(monedas.Adapt<IReadOnlyList<MonedaDto>>());
    }
}
