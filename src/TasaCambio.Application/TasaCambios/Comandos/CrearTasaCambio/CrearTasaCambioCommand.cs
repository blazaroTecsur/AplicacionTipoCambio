using MediatR;
using TasaCambio.Application.Comun.Dtos;

namespace TasaCambio.Application.TasaCambios.Comandos.CrearTasaCambio;

public sealed record CrearTasaCambioCommand(
    string Empresa,
    string CodigoMoneda,
    DateOnly Fecha,
    decimal ValorCompra,
    decimal ValorVenta,
    string? FuenteOrigen = null
) : IRequest<ResponseDto<TasaCambioDto>>;
