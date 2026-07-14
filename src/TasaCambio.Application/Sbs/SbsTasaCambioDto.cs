namespace TasaCambio.Application.Sbs;

public sealed record SbsTasaCambioDto
{
    public string CodigoMoneda { get; init; } = string.Empty;
    public string DescripcionMoneda { get; init; } = string.Empty;
    public string ValorCompra { get; init; } = string.Empty;
    public string ValorVenta { get; init; } = string.Empty;
    public DateOnly Fecha { get; init; }
}
