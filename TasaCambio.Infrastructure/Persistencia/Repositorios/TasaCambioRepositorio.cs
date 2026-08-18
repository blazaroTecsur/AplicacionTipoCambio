using Microsoft.EntityFrameworkCore;
using TasaCambio.Domain.Interfaces;

namespace TasaCambio.Infrastructure.Persistencia.Repositorios;

internal sealed class TasaCambioRepositorio : RepositorioBase<Domain.Entidades.TasaCambio>, ITasaCambioRepositorio
{
    public TasaCambioRepositorio(TasaCambioDbContext contexto) : base(contexto) { }

    public async Task<Domain.Entidades.TasaCambio?> ObtenerPorFechaAsync(string codigoMoneda, DateOnly fecha, CancellationToken ct = default)
        => await _contexto.TasaCambios
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.CodigoMoneda == codigoMoneda.ToUpper() && t.Fecha == fecha, ct);

    public async Task<Domain.Entidades.TasaCambio?> ObtenerUltimaHastaFechaAsync(string codigoMoneda, DateOnly fecha, CancellationToken ct = default)
        => await _contexto.TasaCambios
            .AsNoTracking()
            .Where(t => t.CodigoMoneda == codigoMoneda.ToUpper() && t.Fecha <= fecha)
            .OrderByDescending(t => t.Fecha)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Domain.Entidades.TasaCambio>> ListarPorMonedaAsync(string codigoMoneda, int? anio, int? mes, CancellationToken ct = default)
    {
        var query = _contexto.TasaCambios
            .AsNoTracking()
            .Where(t => t.CodigoMoneda == codigoMoneda.ToUpper());

        if (anio.HasValue) query = query.Where(t => t.Fecha.Year == anio.Value);
        if (mes.HasValue) query = query.Where(t => t.Fecha.Month == mes.Value);

        return await query.OrderByDescending(t => t.Fecha).ToListAsync(ct);
    }
}
