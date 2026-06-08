using Mapster;
using MediatR;
using TasaCambio.Aplicacion.Comun.Dtos;
using TasaCambio.Dominio.Interfaces;

namespace TasaCambio.Aplicacion.TasaCambios.Consultas.ListarTasasCambio;

internal sealed class ListarTasasCambioHandler : IRequestHandler<ListarTasasCambioQuery, RespuestaDto<IReadOnlyList<TasaCambioDto>>>
{
    private readonly IUnidadDeTrabajo _uow;

    public ListarTasasCambioHandler(IUnidadDeTrabajo uow) => _uow = uow;

    public async Task<RespuestaDto<IReadOnlyList<TasaCambioDto>>> Handle(ListarTasasCambioQuery request, CancellationToken ct)
    {
        var tasas = await _uow.TasaCambios.ListarPorEmpresaYMonedaAsync(
            request.Empresa, request.CodigoMoneda, request.Anio, request.Mes, ct);

        return RespuestaDto<IReadOnlyList<TasaCambioDto>>.Ok(tasas.Adapt<IReadOnlyList<TasaCambioDto>>());
    }
}
