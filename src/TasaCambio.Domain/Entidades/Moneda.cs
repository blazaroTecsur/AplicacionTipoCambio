namespace TasaCambio.Domain.Entidades;

public sealed class Moneda : EntidadBase
{
    public string Empresa { get; private set; } = string.Empty;
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public string Simbolo { get; private set; } = string.Empty;
    public string? CodigoSunat { get; private set; }
    public string? DescripcionIso4217 { get; private set; }

    private Moneda() { }

    public static Moneda Crear(string empresa, string codigo, string descripcion, string simbolo, string usuario)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(empresa);
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        return new Moneda
        {
            Empresa = empresa.Trim().ToUpper(),
            Codigo = codigo.Trim().ToUpper(),
            Descripcion = descripcion.Trim(),
            Simbolo = simbolo.Trim(),
            UsuarioReg = usuario
        };
    }

    public void ActualizarDescripcion(string descripcion, string simbolo, string usuario)
    {
        Descripcion = descripcion.Trim();
        Simbolo = simbolo.Trim();
        UsuarioAct = usuario;
        FechaAct = DateTime.UtcNow;
    }

    public void AsignarCodigosSunat(string codigoSunat, string descripcionIso4217)
    {
        CodigoSunat = codigoSunat;
        DescripcionIso4217 = descripcionIso4217;
    }
}
