using System.Linq.Expressions;
using TasaCambio.Dominio.Entidades;
using TasaCambio.Dominio.Especificaciones;

namespace TasaCambio.Dominio.Interfaces;

public interface IRepositorioBase<T> where T : EntidadBase
{
    Task<T?> ObtenerPorIdAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListarTodosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListarAsync(EspecificacionBase<T> especificacion, CancellationToken ct = default);
    Task<T?> PrimerOPredeterminadoAsync(EspecificacionBase<T> especificacion, CancellationToken ct = default);
    Task<T> AgregarAsync(T entidad, CancellationToken ct = default);
    Task ActualizarAsync(T entidad, CancellationToken ct = default);
    Task EliminarAsync(T entidad, CancellationToken ct = default);
    Task<bool> ExisteAsync(Expression<Func<T, bool>> predicado, CancellationToken ct = default);
}
