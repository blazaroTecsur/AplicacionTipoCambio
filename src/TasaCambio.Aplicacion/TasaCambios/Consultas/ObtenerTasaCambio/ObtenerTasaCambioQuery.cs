using MediatR;
using TasaCambio.Aplicacion.Comun.Dtos;

namespace TasaCambio.Aplicacion.TasaCambios.Consultas.ObtenerTasaCambio;

public sealed record ObtenerTasaCambioQuery(string Empresa, string CodigoMoneda, DateOnly Fecha)
    : IRequest<RespuestaDto<TasaCambioDto>>;
