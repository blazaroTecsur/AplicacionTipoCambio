using Microsoft.EntityFrameworkCore;
using TasaCambio.Dominio.Interfaces;

namespace TasaCambio.Infraestructura.Persistencia.Repositorios;

internal sealed class TasaCambioRepositorio : RepositorioBase<Dominio.Entidades.TasaCambio>, ITasaCambioRepositorio
{
    public TasaCambioRepositorio(TasaCambioDbContext contexto) : base(contexto) { }

    public async Task<Dominio.Entidades.TasaCambio?> ObtenerPorFechaAsync(string empresa, string codigoMoneda, DateOnly fecha, CancellationToken ct = default)
        => await _contexto.TasaCambios
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Empresa == empresa.ToUpper() && t.CodigoMoneda == codigoMoneda.ToUpper() && t.Fecha == fecha, ct);

    public async Task<Dominio.Entidades.TasaCambio?> ObtenerUltimaHastaFechaAsync(string empresa, string codigoMoneda, DateOnly fecha, CancellationToken ct = default)
        => await _contexto.TasaCambios
            .AsNoTracking()
            .Where(t => t.Empresa == empresa.ToUpper() && t.CodigoMoneda == codigoMoneda.ToUpper() && t.Fecha <= fecha)
            .OrderByDescending(t => t.Fecha)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Dominio.Entidades.TasaCambio>> ListarPorEmpresaYMonedaAsync(string empresa, string codigoMoneda, int? anio, int? mes, CancellationToken ct = default)
    {
        var query = _contexto.TasaCambios
            .AsNoTracking()
            .Where(t => t.Empresa == empresa.ToUpper() && t.CodigoMoneda == codigoMoneda.ToUpper());

        if (anio.HasValue) query = query.Where(t => t.Fecha.Year == anio.Value);
        if (mes.HasValue) query = query.Where(t => t.Fecha.Month == mes.Value);

        return await query.OrderByDescending(t => t.Fecha).ToListAsync(ct);
    }
}
