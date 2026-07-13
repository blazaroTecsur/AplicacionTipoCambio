using Infor.Abstractions.Interfaces;
using Infor.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Infor.Infrastructure.DependencyInjection;

public static class InforServiceCollectionExtensions
{
    public static IServiceCollection AddInfor(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<InforSettings>(config.GetSection("ApiSettings:Infor"));

        services.AddHttpClient("InforSsoClient")
            .AddTransientHttpErrorPolicy(p =>
                p.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        services.AddHttpClient("InforIdoClient")
            .AddTransientHttpErrorPolicy(p =>
                p.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
            .AddTransientHttpErrorPolicy(p =>
                p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

        services.AddSingleton<IInforTokenService, InforTokenService>();
        services.AddScoped<IInforIdoService, InforIdoService>();

        return services;
    }
}
