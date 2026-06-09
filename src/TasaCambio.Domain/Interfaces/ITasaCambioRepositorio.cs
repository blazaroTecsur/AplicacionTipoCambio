namespace TasaCambio.Domain.Interfaces;

public interface ITasaCambioRepositorio : IRepositorioBase<Entidades.TasaCambio>
{
    Task<Entidades.TasaCambio?> ObtenerPorFechaAsync(string empresa, string codigoMoneda, DateOnly fecha, CancellationToken ct = default);
    Task<Entidades.TasaCambio?> ObtenerUltimaHastaFechaAsync(string empresa, string codigoMoneda, DateOnly fecha, CancellationToken ct = default);
    Task<IReadOnlyList<Entidades.TasaCambio>> ListarPorEmpresaYMonedaAsync(string empresa, string codigoMoneda, int? anio, int? mes, CancellationToken ct = default);
}
