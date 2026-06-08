namespace TasaCambio.Aplicacion.Comun.Interfaces;

public interface IContextoUsuario
{
    string NombreUsuario { get; }
    string? IpCliente { get; }
}
