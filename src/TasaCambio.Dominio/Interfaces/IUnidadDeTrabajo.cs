namespace TasaCambio.Dominio.Interfaces;

public interface IUnidadDeTrabajo : IDisposable
{
    ITasaCambioRepositorio TasaCambios { get; }
    IMonedaRepositorio Monedas { get; }
    Task<int> GuardarCambiosAsync(CancellationToken ct = default);
    Task IniciarTransaccionAsync(CancellationToken ct = default);
    Task ConfirmarTransaccionAsync(CancellationToken ct = default);
    Task RevertirTransaccionAsync(CancellationToken ct = default);
}
