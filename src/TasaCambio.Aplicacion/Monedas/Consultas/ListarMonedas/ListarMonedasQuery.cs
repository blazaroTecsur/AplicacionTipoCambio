using MediatR;
using TasaCambio.Aplicacion.Comun.Dtos;

namespace TasaCambio.Aplicacion.Monedas.Consultas.ListarMonedas;

public sealed record ListarMonedasQuery(string Empresa) : IRequest<RespuestaDto<IReadOnlyList<MonedaDto>>>;
