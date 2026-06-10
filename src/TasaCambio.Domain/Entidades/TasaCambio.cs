using TasaCambio.Domain.Excepciones;

namespace TasaCambio.Domain.Entidades;

public sealed class TasaCambio : EntidadBase
{
    public string Empresa { get; private set; } = string.Empty;
    public string CodigoMoneda { get; private set; } = string.Empty;
    public DateOnly Fecha { get; private set; }
    public decimal ValorCompra { get; private set; }
    public decimal ValorVenta { get; private set; }
    public decimal TasaPromedio => (ValorCompra + ValorVenta) / 2;
    public DateOnly? FechaSbs { get; private set; }
    public string? FuenteOrigen { get; private set; }

    private TasaCambio() { }

    public static TasaCambio Crear(
        string empresa,
        string codigoMoneda,
        DateOnly fecha,
        decimal valorCompra,
        decimal valorVenta,
        string usuario,
        string? fuenteOrigen = null)
    {
        ValidarValores(valorCompra, valorVenta);

        return new TasaCambio
        {
            Empresa = empresa.Trim().ToUpper(),
            CodigoMoneda = codigoMoneda.Trim().ToUpper(),
            Fecha = fecha,
            ValorCompra = valorCompra,
            ValorVenta = valorVenta,
            FuenteOrigen = fuenteOrigen,
            UsuarioReg = usuario
        };
    }

    public void ActualizarValores(decimal valorCompra, decimal valorVenta, string usuario, string? fuenteOrigen = null)
    {
        ValidarValores(valorCompra, valorVenta);
        ValorCompra = valorCompra;
        ValorVenta = valorVenta;
        if (fuenteOrigen is not null) FuenteOrigen = fuenteOrigen;
        UsuarioAct = usuario;
        FechaAct = DateTime.UtcNow;
    }

    public void AsignarFechaSbs(DateOnly fechaSbs) => FechaSbs = fechaSbs;

    private static void ValidarValores(decimal compra, decimal venta)
    {
        if (compra <= 0) throw new DomainException("El valor de compra debe ser mayor a cero.");
        if (venta <= 0) throw new DomainException("El valor de venta debe ser mayor a cero.");
        if (compra > venta) throw new DomainException("El valor de compra no puede ser mayor al de venta.");
    }
}
