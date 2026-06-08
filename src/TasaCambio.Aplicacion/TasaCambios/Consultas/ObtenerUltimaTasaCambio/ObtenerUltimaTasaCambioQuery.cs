using MediatR;
using TasaCambio.Aplicacion.Comun.Dtos;

namespace TasaCambio.Aplicacion.TasaCambios.Consultas.ObtenerUltimaTasaCambio;

public sealed record ObtenerUltimaTasaCambioQuery(string Empresa, string CodigoMoneda, DateOnly HastaFecha)
    : IRequest<RespuestaDto<TasaCambioDto>>;
