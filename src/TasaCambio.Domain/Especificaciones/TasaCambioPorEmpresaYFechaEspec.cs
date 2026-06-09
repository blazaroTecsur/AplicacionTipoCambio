using System.Linq.Expressions;

namespace TasaCambio.Domain.Especificaciones;

public sealed class TasaCambioPorEmpresaYFechaEspec : EspecificacionBase<Entidades.TasaCambio>
{
    private readonly string _empresa;
    private readonly string _codigoMoneda;
    private readonly DateOnly _fecha;

    public TasaCambioPorEmpresaYFechaEspec(string empresa, string codigoMoneda, DateOnly fecha)
    {
        _empresa = empresa.ToUpper();
        _codigoMoneda = codigoMoneda.ToUpper();
        _fecha = fecha;
    }

    public override Expression<Func<Entidades.TasaCambio, bool>> Criterio =>
        t => t.Empresa == _empresa && t.CodigoMoneda == _codigoMoneda && t.Fecha == _fecha;
}
