using TasaCambio.Domain.Excepciones;

namespace TasaCambio.UnitTest.Dominio;

/// <summary>
/// Pruebas unitarias para la entidad de dominio TasaCambio.
/// Valida las reglas de negocio que protegen la integridad del dato
/// tanto en HU-01 (creación automática desde SBS) como en HU-02 (registro en SyteLine).
/// </summary>
public class TasaCambioEntidadTests
{
    private static readonly DateOnly Fecha   = new(2026, 7, 14);
    private const decimal            Compra  = 3.71m;
    private const decimal            Venta   = 3.72m;
    private const string             Usuario = "WORKER";

    // ── Crear: escenarios válidos ─────────────────────────────────────────────

    [Fact]
    public void Crear_ConValoresValidos_RetornaEntidadConDatosCorrectos()
    {
        // Act
        var tasa = TasaCambioEntity.Crear("usd", Fecha, Compra, Venta, Usuario, "SBS");

        // Assert
        tasa.CodigoMoneda.Should().Be("USD",         "el código debe normalizarse a mayúsculas");
        tasa.Fecha.Should().Be(Fecha);
        tasa.ValorCompra.Should().Be(Compra);
        tasa.ValorVenta.Should().Be(Venta);
        tasa.FuenteOrigen.Should().Be("SBS");
        tasa.UsuarioReg.Should().Be(Usuario);
    }

    [Fact]
    public void Crear_CodigoMonedaConEspacios_SeNormalizaToUpperTrim()
    {
        // Act
        var tasa = TasaCambioEntity.Crear("  eur  ", Fecha, Compra, Venta, Usuario);

        // Assert
        tasa.CodigoMoneda.Should().Be("EUR");
    }

    [Fact]
    public void Crear_SinFuenteOrigen_FuenteOrigenEsNull()
    {
        var tasa = TasaCambioEntity.Crear("USD", Fecha, Compra, Venta, Usuario);

        tasa.FuenteOrigen.Should().BeNull();
    }

    // ── Crear: validaciones de dominio ────────────────────────────────────────

    [Fact]
    public void Crear_CompraEnCero_LanzaDomainException()
    {
        // Compra = 0 no es un valor válido
        var act = () => TasaCambioEntity.Crear("USD", Fecha, valorCompra: 0m, valorVenta: Venta, usuario: Usuario);

        act.Should().Throw<DomainException>()
           .WithMessage("*compra*");
    }

    [Fact]
    public void Crear_CompraNegativa_LanzaDomainException()
    {
        var act = () => TasaCambioEntity.Crear("USD", Fecha, valorCompra: -1m, valorVenta: Venta, usuario: Usuario);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Crear_VentaEnCero_LanzaDomainException()
    {
        var act = () => TasaCambioEntity.Crear("USD", Fecha, valorCompra: Compra, valorVenta: 0m, usuario: Usuario);

        act.Should().Throw<DomainException>()
           .WithMessage("*venta*");
    }

    [Fact]
    public void Crear_CompraMayorQueVenta_LanzaDomainException()
    {
        // Invariante de negocio: compra nunca puede superar venta
        var act = () => TasaCambioEntity.Crear("USD", Fecha, valorCompra: 3.80m, valorVenta: 3.70m, usuario: Usuario);

        act.Should().Throw<DomainException>()
           .WithMessage("*compra*mayor*venta*", "la regla de negocio exige compra <= venta");
    }

    [Fact]
    public void Crear_CompraIgualAVenta_EsValido()
    {
        // Borde: compra == venta es permitido
        var act = () => TasaCambioEntity.Crear("USD", Fecha, valorCompra: 3.72m, valorVenta: 3.72m, usuario: Usuario);

        act.Should().NotThrow();
    }

    // ── TasaPromedio ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(3.71, 3.72, 3.715)]
    [InlineData(3.50, 3.60, 3.55)]
    [InlineData(4.00, 4.00, 4.00)]
    public void TasaPromedio_EsPromedioAritmeticoDeCompraYVenta(double compra, double venta, double promedio)
    {
        var tasa = TasaCambioEntity.Crear("USD", Fecha, (decimal)compra, (decimal)venta, Usuario);

        tasa.TasaPromedio.Should().Be((decimal)promedio);
    }

    // ── ActualizarValores ─────────────────────────────────────────────────────

    [Fact]
    public void ActualizarValores_ConNuevosValoresValidos_ActualizaCompraVentaYUsuario()
    {
        // Arrange
        var tasa = TasaCambioEntity.Crear("USD", Fecha, 3.69m, 3.70m, "WORKER_INIT");

        // Act
        tasa.ActualizarValores(3.71m, 3.72m, "WORKER_UPDATE", "SBS");

        // Assert
        tasa.ValorCompra.Should().Be(3.71m);
        tasa.ValorVenta.Should().Be(3.72m);
        tasa.UsuarioAct.Should().Be("WORKER_UPDATE");
        tasa.FechaAct.Should().NotBeNull();
        tasa.FuenteOrigen.Should().Be("SBS");
    }

    [Fact]
    public void ActualizarValores_CompraEnCero_LanzaDomainException()
    {
        var tasa = TasaCambioEntity.Crear("USD", Fecha, Compra, Venta, Usuario);

        var act = () => tasa.ActualizarValores(valorCompra: 0m, valorVenta: Venta, usuario: Usuario);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ActualizarValores_CompraMayorQueNuevaVenta_LanzaDomainException()
    {
        var tasa = TasaCambioEntity.Crear("USD", Fecha, Compra, Venta, Usuario);

        var act = () => tasa.ActualizarValores(valorCompra: 3.80m, valorVenta: 3.70m, usuario: Usuario);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ActualizarValores_SinFuenteOrigen_NoModificaFuenteExistente()
    {
        var tasa = TasaCambioEntity.Crear("USD", Fecha, Compra, Venta, Usuario, "SBS");

        tasa.ActualizarValores(3.71m, 3.72m, Usuario, fuenteOrigen: null);

        tasa.FuenteOrigen.Should().Be("SBS", "si no se pasa fuente origen, debe preservar la anterior");
    }

    // ── AsignarFechaSbs ───────────────────────────────────────────────────────

    [Fact]
    public void AsignarFechaSbs_GuardaLaFechaPublicadaPorSbs()
    {
        var tasa     = TasaCambioEntity.Crear("USD", Fecha, Compra, Venta, Usuario);
        var fechaSbs = new DateOnly(2026, 7, 13);

        tasa.AsignarFechaSbs(fechaSbs);

        tasa.FechaSbs.Should().Be(fechaSbs);
    }
}
