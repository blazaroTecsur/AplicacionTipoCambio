using MediatR;
using TasaCambio.Application.Comun.Dtos;

namespace TasaCambio.Application.Monedas.Consultas.ListarMonedas;

public sealed record ListarMonedasQuery(string Empresa) : IRequest<ResponseDto<IReadOnlyList<MonedaDto>>>;
