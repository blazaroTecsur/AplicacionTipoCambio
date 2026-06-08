using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using TasaCambio.Aplicacion.Comun.Interfaces;
using TasaCambio.Dominio.Interfaces;
using TasaCambio.Infraestructura.Auditoria;
using TasaCambio.Infraestructura.Persistencia;

namespace TasaCambio.Infraestructura;

public static class DependencyInjection
{
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Cadena de conexión 'DefaultConnection' no configurada.");

        services.AddDbContext<TasaCambioDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();
        services.AddScoped<IServicioAuditoria, ServicioAuditoria>();

        services.AddHttpClient("SbsClient")
            .AddTransientHttpErrorPolicy(p =>
                p.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
            .AddTransientHttpErrorPolicy(p =>
                p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

        return services;
    }
}
