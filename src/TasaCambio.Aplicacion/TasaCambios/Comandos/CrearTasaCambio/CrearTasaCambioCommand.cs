using MediatR;
using TasaCambio.Aplicacion.Comun.Dtos;

namespace TasaCambio.Aplicacion.TasaCambios.Comandos.CrearTasaCambio;

public sealed record CrearTasaCambioCommand(
    string Empresa,
    string CodigoMoneda,
    DateOnly Fecha,
    decimal ValorCompra,
    decimal ValorVenta,
    string? FuenteOrigen = null
) : IRequest<RespuestaDto<TasaCambioDto>>;
