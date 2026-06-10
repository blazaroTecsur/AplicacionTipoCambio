using MediatR;
using TasaCambio.Application.Comun.Dtos;

namespace TasaCambio.Application.Monedas.Consultas.ListarMonedas;

public sealed record ListarMonedasQuery : IRequest<ResponseDto<IReadOnlyList<MonedaDto>>>;
