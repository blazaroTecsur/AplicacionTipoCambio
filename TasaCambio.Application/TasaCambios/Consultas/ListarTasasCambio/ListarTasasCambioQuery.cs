using MediatR;
using TasaCambio.Application.Comun.Dtos;

namespace TasaCambio.Application.TasaCambios.Consultas.ListarTasasCambio;

public sealed record ListarTasasCambioQuery(string CodigoMoneda, int? Anio = null, int? Mes = null)
    : IRequest<ResponseDto<IReadOnlyList<TasaCambioDto>>>;
