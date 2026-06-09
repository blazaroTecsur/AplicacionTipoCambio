namespace TasaCambio.Presentation.Middleware;

internal sealed class AutenticacionApiKey : IMiddleware
{
    private const string HeaderApiKey = "X-Api-Key";
    private readonly IConfiguration _config;

    public AutenticacionApiKey(IConfiguration config) => _config = config;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/health"))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderApiKey, out var apiKeyRecibida))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API Key requerida.");
            return;
        }

        var apiKeyEsperada = _config["Seguridad:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKeyEsperada) || apiKeyRecibida != apiKeyEsperada)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API Key inválida.");
            return;
        }

        await next(context);
    }
}
