namespace TasaCambio.Application.Comun.Interfaces;

public interface IContextoUsuario
{
    string NombreUsuario { get; }
    string? IpCliente { get; }
}
