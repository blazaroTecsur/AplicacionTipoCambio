using TasaCambio.Aplicacion.Comun.Interfaces;

namespace TasaCambio.Presentacion.Middleware;

internal sealed class ContextoUsuarioHttp : IContextoUsuario
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ContextoUsuarioHttp(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public string NombreUsuario =>
        _httpContextAccessor.HttpContext?.Request.Headers["X-Usuario"].FirstOrDefault() ?? "sistema";

    public string? IpCliente =>
        _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
