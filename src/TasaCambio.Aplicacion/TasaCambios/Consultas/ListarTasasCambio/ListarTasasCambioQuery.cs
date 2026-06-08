using MediatR;
using TasaCambio.Aplicacion.Comun.Dtos;

namespace TasaCambio.Aplicacion.TasaCambios.Consultas.ListarTasasCambio;

public sealed record ListarTasasCambioQuery(string Empresa, string CodigoMoneda, int? Anio = null, int? Mes = null)
    : IRequest<RespuestaDto<IReadOnlyList<TasaCambioDto>>>;
