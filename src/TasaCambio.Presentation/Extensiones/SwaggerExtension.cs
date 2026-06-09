using Microsoft.OpenApi.Models;

namespace TasaCambio.Presentation.Extensiones;

internal static class SwaggerExtension
{
    internal static IServiceCollection AgregarSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TasaCambio API",
                Version = "v1",
                Description = "Microservicio de gestión de tasas de cambio"
            });

            c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Name = "X-Api-Key",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Description = "API Key requerida"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
                    },
                    []
                }
            });
        });

        return services;
    }
}
