using MediatR;
using TasaCambio.Application.Comun.Dtos;

namespace TasaCambio.Application.TasaCambios.Consultas.ObtenerTasaCambio;

public sealed record ObtenerTasaCambioQuery(string CodigoMoneda, DateOnly Fecha)
    : IRequest<ResponseDto<TasaCambioDto>>;
