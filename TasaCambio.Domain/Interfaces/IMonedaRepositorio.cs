namespace TasaCambio.Domain.Interfaces;

public interface IMonedaRepositorio : IRepositorioBase<Entidades.Moneda>
{
    Task<Entidades.Moneda?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default);
    Task<IReadOnlyList<Entidades.Moneda>> ListarTodasAsync(CancellationToken ct = default);
}
