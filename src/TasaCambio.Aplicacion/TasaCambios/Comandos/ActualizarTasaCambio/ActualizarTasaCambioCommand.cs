using MediatR;
using TasaCambio.Aplicacion.Comun.Dtos;

namespace TasaCambio.Aplicacion.TasaCambios.Comandos.ActualizarTasaCambio;

public sealed record ActualizarTasaCambioCommand(long Id, decimal ValorCompra, decimal ValorVenta)
    : IRequest<RespuestaDto<TasaCambioDto>>;
