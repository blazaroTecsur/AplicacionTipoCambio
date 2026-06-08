namespace TasaCambio.Aplicacion.TasaCambios;

public sealed record TasaCambioDto
{
    public long Id { get; init; }
    public string Empresa { get; init; } = string.Empty;
    public string CodigoMoneda { get; init; } = string.Empty;
    public DateOnly Fecha { get; init; }
    public decimal ValorCompra { get; init; }
    public decimal ValorVenta { get; init; }
    public decimal TasaPromedio { get; init; }
    public string? FuenteOrigen { get; init; }
}
