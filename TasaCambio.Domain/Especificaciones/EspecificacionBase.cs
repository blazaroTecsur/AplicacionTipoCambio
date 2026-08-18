using System.Linq.Expressions;

namespace TasaCambio.Domain.Especificaciones;

public abstract class EspecificacionBase<T>
{
    public abstract Expression<Func<T, bool>> Criterio { get; }

    public bool EsSatisfechaPor(T entidad) => Criterio.Compile()(entidad);
}
