using MediatR;
using TasaCambio.Application.Comun.Dtos;

namespace TasaCambio.Application.TasaCambios.Comandos.EliminarTasaCambio;

public sealed record EliminarTasaCambioCommand(long Id) : IRequest<ResponseDto<bool>>;
