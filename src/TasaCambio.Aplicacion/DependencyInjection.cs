using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TasaCambio.Aplicacion.Comun.Comportamientos;

namespace TasaCambio.Aplicacion;

public static class DependencyInjection
{
    public static IServiceCollection AgregarAplicacion(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ComportamientoLogging<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ComportamientoValidacion<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
