using MediatR;
using TasaCambio.Application.Comun.Dtos;

namespace TasaCambio.Application.TasaCambios.Consultas.ObtenerUltimaTasaCambio;

public sealed record ObtenerUltimaTasaCambioQuery(string Empresa, string CodigoMoneda, DateOnly HastaFecha)
    : IRequest<ResponseDto<TasaCambioDto>>;
