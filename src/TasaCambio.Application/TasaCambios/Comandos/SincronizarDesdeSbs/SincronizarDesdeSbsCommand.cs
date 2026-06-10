using MediatR;
using TasaCambio.Application.Comun.Dtos;

namespace TasaCambio.Application.TasaCambios.Comandos.SincronizarDesdeSbs;

public sealed record SincronizarDesdeSbsCommand(string CodigoMoneda, DateOnly Fecha) : IRequest<ResponseDto<TasaCambioDto>>;
