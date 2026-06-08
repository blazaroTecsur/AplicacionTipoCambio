using FluentValidation;
using MediatR;

namespace TasaCambio.Aplicacion.Comun.Comportamientos;

public sealed class ComportamientoValidacion<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validadores;

    public ComportamientoValidacion(IEnumerable<IValidator<TRequest>> validadores)
        => _validadores = validadores;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!_validadores.Any()) return await next();

        var contexto = new ValidationContext<TRequest>(request);
        var resultados = await Task.WhenAll(_validadores.Select(v => v.ValidateAsync(contexto, ct)));

        var errores = resultados
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (errores.Count != 0) throw new ValidationException(errores);

        return await next();
    }
}
