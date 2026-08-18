using MediatR;
using Microsoft.Extensions.Logging;

namespace TasaCambio.Application.Comun.Comportamientos;

public sealed class ComportamientoLogging<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<ComportamientoLogging<TRequest, TResponse>> _logger;

    public ComportamientoLogging(ILogger<ComportamientoLogging<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var nombre = typeof(TRequest).Name;
        _logger.LogInformation("Ejecutando {Request}", nombre);

        try
        {
            var respuesta = await next();
            _logger.LogInformation("Completado {Request}", nombre);
            return respuesta;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en {Request}", nombre);
            throw;
        }
    }
}
