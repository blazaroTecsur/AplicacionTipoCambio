namespace TasaCambio.Aplicacion.Comun.Interfaces;

public interface IServicioAuditoria
{
    Task RegistrarAsync(string accion, string entidad, object? datos = null, CancellationToken ct = default);
}
