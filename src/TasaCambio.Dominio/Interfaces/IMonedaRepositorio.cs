namespace TasaCambio.Dominio.Interfaces;

public interface IMonedaRepositorio : IRepositorioBase<Entidades.Moneda>
{
    Task<Entidades.Moneda?> ObtenerPorCodigoAsync(string empresa, string codigo, CancellationToken ct = default);
    Task<IReadOnlyList<Entidades.Moneda>> ListarPorEmpresaAsync(string empresa, CancellationToken ct = default);
}
