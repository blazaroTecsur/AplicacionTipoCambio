using Infor.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using TasaCambio.Application.Comun.Interfaces;
using TasaCambio.Domain.Interfaces;
using TasaCambio.Infrastructure.Auditoria;
using TasaCambio.Infrastructure.Persistencia;
using TasaCambio.Infrastructure.Servicios;

namespace TasaCambio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Cadena de conexión 'DefaultConnection' no configurada.");

        var serverVersion = new MySqlServerVersion(new Version(8, 3, 0));

        services.AddDbContext<TasaCambioDbContext>(options =>
            options.UseMySql(connectionString, serverVersion, mySqlOptions =>
                mySqlOptions.EnableRetryOnFailure()));

        services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();
        services.AddScoped<IServicioAuditoria, ServicioAuditoria>();

        // ── SBS (fuente de tipo de cambio) ────────────────────────────────────
        services.AddHttpClient("SbsXmlClient")
            .AddTransientHttpErrorPolicy(p =>
                p.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
            .AddTransientHttpErrorPolicy(p =>
                p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

        services.AddHttpClient("SbsHtmlClient")
            .AddTransientHttpErrorPolicy(p =>
                p.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
            .AddTransientHttpErrorPolicy(p =>
                p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

        services.AddScoped<IServicioSbs, ServicioSbs>();

        // ── Infor SyteLine IDO ────────────────────────────────────────────────
        services.AddInfor(config);
        services.AddScoped<IServicioSyteline, ServicioSyteline>();

        return services;
    }
}
