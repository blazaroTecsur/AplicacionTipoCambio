using System.Text.Json.Serialization;

namespace TasaCambio.Application.Sbs;

public sealed record SbsTasaCambioDto
{
    [JsonPropertyName("codigo_moneda")]
    public string CodigoMoneda { get; init; } = string.Empty;
    [JsonPropertyName("descripcion_moneda")]
    public string DescripcionMoneda { get; init; } = string.Empty;
    [JsonPropertyName("valor_compra")]
    public string ValorCompra { get; init; } = string.Empty;
    [JsonPropertyName("valor_venta")]
    public string ValorVenta { get; init; } = string.Empty;
}
