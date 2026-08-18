using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TasaCambio.Application.Comun.Comportamientos;

namespace TasaCambio.Application;

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
