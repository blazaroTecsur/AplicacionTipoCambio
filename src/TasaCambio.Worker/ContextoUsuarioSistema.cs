using TasaCambio.Application.Comun.Interfaces;

namespace TasaCambio.Worker;

internal sealed class ContextoUsuarioSistema : IContextoUsuario
{
    public string NombreUsuario => "WORKER";
}
