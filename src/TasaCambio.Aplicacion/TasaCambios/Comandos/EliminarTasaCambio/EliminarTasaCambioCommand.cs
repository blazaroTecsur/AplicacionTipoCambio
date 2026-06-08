using MediatR;
using TasaCambio.Aplicacion.Comun.Dtos;

namespace TasaCambio.Aplicacion.TasaCambios.Comandos.EliminarTasaCambio;

public sealed record EliminarTasaCambioCommand(long Id) : IRequest<RespuestaDto<bool>>;
