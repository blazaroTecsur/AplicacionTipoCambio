using System.Text.Json;
using Microsoft.Extensions.Logging;
using TasaCambio.Aplicacion.Comun.Interfaces;

namespace TasaCambio.Infraestructura.Auditoria;

internal sealed class ServicioAuditoria : IServicioAuditoria
{
    private readonly ILogger<ServicioAuditoria> _logger;
    private readonly IContextoUsuario _contextoUsuario;

    public ServicioAuditoria(ILogger<ServicioAuditoria> logger, IContextoUsuario contextoUsuario)
    {
        _logger = logger;
        _contextoUsuario = contextoUsuario;
    }

    public Task RegistrarAsync(string accion, string entidad, object? datos = null, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "AUDITORIA | Accion={Accion} Entidad={Entidad} Usuario={Usuario} IP={IP} Datos={Datos}",
            accion,
            entidad,
            _contextoUsuario.NombreUsuario,
            _contextoUsuario.IpCliente,
            datos is null ? null : JsonSerializer.Serialize(datos));

        return Task.CompletedTask;
    }
}
