using Mapster;
using MediatR;
using TasaCambio.Application.Comun.Dtos;
using TasaCambio.Application.Monedas;
using TasaCambio.Domain.Comun;
using TasaCambio.Domain.Excepciones;
using TasaCambio.Domain.Interfaces;

namespace TasaCambio.Application.TasaCambios.Consultas.ObtenerTasaCambio;

internal sealed class ObtenerTasaCambioHandler : IRequestHandler<ObtenerTasaCambioQuery, ResponseDto<TasaCambioDto>>
{
    private readonly IUnidadDeTrabajo _uow;

    public ObtenerTasaCambioHandler(IUnidadDeTrabajo uow) => _uow = uow;

    public async Task<ResponseDto<TasaCambioDto>> Handle(ObtenerTasaCambioQuery request, CancellationToken ct)
    {
        var detalleMoneda = (await _uow.Monedas.ObtenerPorCodigoAsync(request.CodigoMoneda, ct))
            ?.Adapt<MonedaDto>();

        if (request.CodigoMoneda.Trim().Equals(Constantes.CodigoMonedaNacional, StringComparison.OrdinalIgnoreCase))
        {
            return ResponseDto<TasaCambioDto>.Ok(new TasaCambioDto
            {
                CodigoMoneda = Constantes.CodigoMonedaNacional,
                Fecha = request.Fecha,
                ValorCompra = 1m,
                ValorVenta = 1m,
                TasaPromedio = 1m,
                FechaSbs = request.Fecha,
                DetalleMoneda = detalleMoneda
            });
        }

        var tasa = await _uow.TasaCambios.ObtenerPorFechaAsync(request.CodigoMoneda, request.Fecha, ct)
            ?? throw new NotFoundException(nameof(Domain.Entidades.TasaCambio), $"{request.CodigoMoneda}/{request.Fecha}");

        var dto = tasa.Adapt<TasaCambioDto>() with { DetalleMoneda = detalleMoneda };
        return ResponseDto<TasaCambioDto>.Ok(dto);
    }
}
