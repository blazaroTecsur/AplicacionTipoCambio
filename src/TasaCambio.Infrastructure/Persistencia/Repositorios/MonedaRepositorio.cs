using Microsoft.EntityFrameworkCore;
using TasaCambio.Domain.Entidades;
using TasaCambio.Domain.Interfaces;

namespace TasaCambio.Infrastructure.Persistencia.Repositorios;

internal sealed class MonedaRepositorio : RepositorioBase<Moneda>, IMonedaRepositorio
{
    public MonedaRepositorio(TasaCambioDbContext contexto) : base(contexto) { }

    public async Task<Moneda?> ObtenerPorCodigoAsync(string empresa, string codigo, CancellationToken ct = default)
        => await _contexto.Monedas
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Empresa == empresa.ToUpper() && m.Codigo == codigo.ToUpper(), ct);

    public async Task<IReadOnlyList<Moneda>> ListarPorEmpresaAsync(string empresa, CancellationToken ct = default)
        => await _contexto.Monedas
            .AsNoTracking()
            .Where(m => m.Empresa == empresa.ToUpper())
            .OrderBy(m => m.Codigo)
            .ToListAsync(ct);
}
