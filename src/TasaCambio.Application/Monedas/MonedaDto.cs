namespace TasaCambio.Application.Monedas;

/// <summary>Datos de una moneda.</summary>
public sealed record MonedaDto
{
    /// <summary>Identificador único.</summary>
    /// <example>1</example>
    public long Id { get; init; }

    /// <summary>Código de la moneda.</summary>
    /// <example>USD</example>
    public string Codigo { get; init; } = string.Empty;

    /// <summary>Descripción de la moneda.</summary>
    /// <example>Dólar Americano</example>
    public string Descripcion { get; init; } = string.Empty;

    /// <summary>Símbolo de la moneda.</summary>
    /// <example>$</example>
    public string Simbolo { get; init; } = string.Empty;

    /// <summary>Código SUNAT.</summary>
    /// <example>002</example>
    public string? CodigoSunat { get; init; }

    /// <summary>Descripción ISO 4217.</summary>
    /// <example>US Dollar</example>
    public string? DescripcionIso4217 { get; init; }
}
