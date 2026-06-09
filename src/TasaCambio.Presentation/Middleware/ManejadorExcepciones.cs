using System.Text.Json;
using FluentValidation;
using TasaCambio.Domain.Excepciones;

namespace TasaCambio.Presentation.Middleware;

internal sealed class ManejadorExcepciones : IMiddleware
{
    private readonly ILogger<ManejadorExcepciones> _logger;

    public ManejadorExcepciones(ILogger<ManejadorExcepciones> logger) => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado: {Message}", ex.Message);
            await EscribirRespuestaAsync(context, ex);
        }
    }

    private static async Task EscribirRespuestaAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, errores) = ex switch
        {
            ValidationException ve => (StatusCodes.Status400BadRequest,
                ve.Errors.Select(e => e.ErrorMessage).ToList()),
            NotFoundException nfe => (StatusCodes.Status404NotFound, new List<string> { nfe.Message }),
            DomainException de => (StatusCodes.Status422UnprocessableEntity, new List<string> { de.Message }),
            _ => (StatusCodes.Status500InternalServerError, new List<string> { "Ocurrió un error interno." })
        };

        context.Response.StatusCode = statusCode;

        var respuesta = new { exitoso = false, errores };
        await context.Response.WriteAsync(JsonSerializer.Serialize(respuesta));
    }
}
