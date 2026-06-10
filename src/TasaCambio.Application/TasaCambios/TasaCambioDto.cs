using TasaCambio.Application.Monedas;

namespace TasaCambio.Application.TasaCambios;

/// <summary>Datos de una tasa de cambio.</summary>
public sealed record TasaCambioDto
{
    /// <summary>Identificador único.</summary>
    /// <example>1</example>
    public long Id { get; init; }

    /// <summary>Código de la empresa.</summary>
    /// <example>BCP</example>
    public string Empresa { get; init; } = string.Empty;

    /// <summary>Código ISO de la moneda.</summary>
    /// <example>USD</example>
    public string CodigoMoneda { get; init; } = string.Empty;

    /// <summary>Fecha de la tasa.</summary>
    /// <example>2024-06-08</example>
    public DateOnly Fecha { get; init; }

    /// <summary>Valor de compra.</summary>
    /// <example>3.50</example>
    public decimal ValorCompra { get; init; }

    /// <summary>Valor de venta.</summary>
    /// <example>3.52</example>
    public decimal ValorVenta { get; init; }

    /// <summary>Promedio entre compra y venta.</summary>
    /// <example>3.51</example>
    public decimal TasaPromedio { get; init; }

    /// <summary>Fuente de origen del dato.</summary>
    /// <example>SBS</example>
    public string? FuenteOrigen { get; init; }

    /// <summary>Fecha de la tasa obtenida desde SBS.</summary>
    /// <example>2024-06-08</example>
    public DateOnly? FechaSbs { get; init; }

    /// <summary>Detalle de la moneda asociada.</summary>
    public MonedaDto? DetalleMoneda { get; init; }
}
