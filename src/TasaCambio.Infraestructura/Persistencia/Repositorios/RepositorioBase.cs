using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TasaCambio.Dominio.Entidades;
using TasaCambio.Dominio.Especificaciones;
using TasaCambio.Dominio.Interfaces;

namespace TasaCambio.Infraestructura.Persistencia.Repositorios;

internal abstract class RepositorioBase<T> : IRepositorioBase<T> where T : EntidadBase
{
    protected readonly TasaCambioDbContext _contexto;

    protected RepositorioBase(TasaCambioDbContext contexto) => _contexto = contexto;

    public async Task<T?> ObtenerPorIdAsync(long id, CancellationToken ct = default)
        => await _contexto.Set<T>().FindAsync([id], ct);

    public async Task<IReadOnlyList<T>> ListarTodosAsync(CancellationToken ct = default)
        => await _contexto.Set<T>().AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<T>> ListarAsync(EspecificacionBase<T> especificacion, CancellationToken ct = default)
        => await _contexto.Set<T>().AsNoTracking().Where(especificacion.Criterio).ToListAsync(ct);

    public async Task<T?> PrimerOPredeterminadoAsync(EspecificacionBase<T> especificacion, CancellationToken ct = default)
        => await _contexto.Set<T>().AsNoTracking().FirstOrDefaultAsync(especificacion.Criterio, ct);

    public async Task<T> AgregarAsync(T entidad, CancellationToken ct = default)
    {
        await _contexto.Set<T>().AddAsync(entidad, ct);
        return entidad;
    }

    public Task ActualizarAsync(T entidad, CancellationToken ct = default)
    {
        _contexto.Set<T>().Update(entidad);
        return Task.CompletedTask;
    }

    public Task EliminarAsync(T entidad, CancellationToken ct = default)
    {
        _contexto.Set<T>().Remove(entidad);
        return Task.CompletedTask;
    }

    public async Task<bool> ExisteAsync(Expression<Func<T, bool>> predicado, CancellationToken ct = default)
        => await _contexto.Set<T>().AnyAsync(predicado, ct);
}
