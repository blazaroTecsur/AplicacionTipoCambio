namespace TasaCambio.Application.Monedas;

public sealed record MonedaDto
{
    public long Id { get; init; }
    public string Empresa { get; init; } = string.Empty;
    public string Codigo { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public string Simbolo { get; init; } = string.Empty;
    public string? CodigoSunat { get; init; }
    public string? DescripcionIso4217 { get; init; }
}
