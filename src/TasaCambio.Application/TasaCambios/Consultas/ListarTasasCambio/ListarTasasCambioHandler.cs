using Mapster;
using MediatR;
using TasaCambio.Application.Comun.Dtos;
using TasaCambio.Application.Monedas;
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

        var detalleMoneda = (await _uow.Monedas.ObtenerPorCodigoAsync(request.Empresa, request.CodigoMoneda, ct))
            ?.Adapt<MonedaDto>();

        var dtos = tasas.Adapt<List<TasaCambioDto>>()
            .Select(dto => dto with { DetalleMoneda = detalleMoneda })
            .ToList();

        return ResponseDto<IReadOnlyList<TasaCambioDto>>.Ok(dtos);
    }
}
