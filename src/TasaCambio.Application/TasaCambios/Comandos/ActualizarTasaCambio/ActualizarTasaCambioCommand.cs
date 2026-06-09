using MediatR;
using TasaCambio.Application.Comun.Dtos;

namespace TasaCambio.Application.TasaCambios.Comandos.ActualizarTasaCambio;

public sealed record ActualizarTasaCambioCommand(long Id, decimal ValorCompra, decimal ValorVenta)
    : IRequest<ResponseDto<TasaCambioDto>>;
