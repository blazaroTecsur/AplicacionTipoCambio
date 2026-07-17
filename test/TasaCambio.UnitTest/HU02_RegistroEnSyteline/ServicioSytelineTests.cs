using Infor.Abstractions.DTOs;
using Infor.Abstractions.Interfaces;
using Infor.Infrastructure.Services;
using Microsoft.Extensions.Options;
using TasaCambio.Infrastructure.Servicios;

namespace TasaCambio.UnitTest.HU02_RegistroEnSyteline;

/// <summary>
/// Pruebas unitarias para HU-02: Registro automático del tipo de cambio en SyteLine via IDO.
/// Cubre los criterios de aceptación de ServicioSyteline (IDO SLCurrates).
/// </summary>
public class ServicioSytelineTests
{
    // ── Mocks ─────────────────────────────────────────────────────────────────
    private readonly Mock<IInforIdoService> _idoMock = new();

    // ── Datos de prueba comunes ───────────────────────────────────────────────
    private static readonly DateOnly   Fecha    = new(2026, 7, 14);
    private const string               Moneda   = "USD";
    private const decimal              Compra   = 3.71m;
    private const decimal              Venta    = 3.72m;
    private const string               Usuario  = "WORKER";
    private const string               ItemIdFicticio = "PBT=AAABXg==";

    private ServicioSyteline CrearServicio(string monedaBase = "PEN")
    {
        var settings = Options.Create(new InforSettings
        {
            MonedaBase = monedaBase,
            AppId      = "TEST_CONFIG",
        });
        return new ServicioSyteline(_idoMock.Object, settings, NullLogger<ServicioSyteline>.Instance);
    }

    // ── CA-1: no existe en IDO → llama InsertItemAsync ───────────────────────

    [Fact]
    public async Task RegistrarTasaCambio_CuandoNoExisteEnIdo_EjecutaInsert()
    {
        // Arrange — LoadAsync retorna lista vacía (no existe el registro)
        _idoMock.Setup(i => i.LoadAsync("SLCurrates", "_ItemId",
                    It.IsAny<string>(), 1, "EffDate DESC", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IdoResponse { Success = true, Items = [] });

        _idoMock.Setup(i => i.InsertItemAsync("SLCurrates", It.IsAny<IEnumerable<IdoProperty>>(),
                    It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IdoResponse { Success = true });

        var svc = CrearServicio();

        // Act
        var ok = await svc.RegistrarTasaCambioAsync(Moneda, Fecha, Compra, Venta, Usuario);

        // Assert
        ok.Should().BeTrue();
        _idoMock.Verify(i => i.InsertItemAsync("SLCurrates", It.IsAny<IEnumerable<IdoProperty>>(),
            It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once,
            "debe llamar InsertItemAsync cuando no existe el registro en SLCurrates");
        _idoMock.Verify(i => i.UpdateItemAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<IEnumerable<IdoProperty>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never,
            "no debe llamar UpdateItemAsync cuando es una inserción");
    }

    // ── CA-1b: existe en IDO → llama UpdateItemAsync con el _ItemId correcto ──

    [Fact]
    public async Task RegistrarTasaCambio_CuandoExisteEnIdo_EjecutaUpdateConItemId()
    {
        // Arrange — LoadAsync devuelve un item con _ItemId
        _idoMock.Setup(i => i.LoadAsync("SLCurrates", "_ItemId",
                    It.IsAny<string>(), 1, "EffDate DESC", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IdoResponse
                {
                    Success = true,
                    Items   = [new Dictionary<string, string?> { { "_ItemId", ItemIdFicticio } }]
                });

        string? itemIdCapturado = null;
        _idoMock.Setup(i => i.UpdateItemAsync("SLCurrates", It.IsAny<string>(),
                    It.IsAny<IEnumerable<IdoProperty>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, IEnumerable<IdoProperty>, bool, CancellationToken>(
                    (_, itemId, _, _, _) => itemIdCapturado = itemId)
                .ReturnsAsync(new IdoResponse { Success = true });

        var svc = CrearServicio();

        // Act
        var ok = await svc.RegistrarTasaCambioAsync(Moneda, Fecha, Compra, Venta, Usuario);

        // Assert
        ok.Should().BeTrue();
        _idoMock.Verify(i => i.UpdateItemAsync("SLCurrates", It.IsAny<string>(),
            It.IsAny<IEnumerable<IdoProperty>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        _idoMock.Verify(i => i.InsertItemAsync(It.IsAny<string>(), It.IsAny<IEnumerable<IdoProperty>>(),
            It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);

        itemIdCapturado.Should().Be(ItemIdFicticio, "el _ItemId obtenido del Load debe usarse en el Update");
    }

    // ── CA-2: Insert incluye los campos clave ToCurrCode, FromCurrCode, EffDate ─

    [Fact]
    public async Task RegistrarTasaCambio_EnInsert_InclueyeTodesLosCamposRequeridos()
    {
        // Arrange
        _idoMock.Setup(i => i.LoadAsync("SLCurrates", "_ItemId",
                    It.IsAny<string>(), 1, "EffDate DESC", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IdoResponse { Success = true, Items = [] });

        List<IdoProperty>? propsCapturadas = null;
        _idoMock.Setup(i => i.InsertItemAsync("SLCurrates", It.IsAny<IEnumerable<IdoProperty>>(),
                    It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Callback<string, IEnumerable<IdoProperty>, bool, CancellationToken>(
                    (_, props, _, _) => propsCapturadas = props.ToList())
                .ReturnsAsync(new IdoResponse { Success = true });

        var svc = CrearServicio();

        // Act
        await svc.RegistrarTasaCambioAsync(Moneda, Fecha, Compra, Venta, Usuario);

        // Assert — todos los campos del IDO SLCurrates según HU-02
        propsCapturadas.Should().NotBeNull();
        propsCapturadas.Should().Contain(p => p.Name == "ToCurrCode"   && p.Value == "PEN",
            "ToCurrCode siempre es PEN (moneda destino)");
        propsCapturadas.Should().Contain(p => p.Name == "FromCurrCode" && p.Value == "USD",
            "FromCurrCode es la moneda de origen (USD o EUR)");
        propsCapturadas.Should().Contain(p => p.Name == "EffDate"      && p.Value == "2026-07-14",
            "EffDate en formato yyyy-MM-dd");
        propsCapturadas.Should().Contain(p => p.Name == "BuyRate"      && p.Value == "3.7100",
            "BuyRate con 4 decimales");
        propsCapturadas.Should().Contain(p => p.Name == "SellRate"     && p.Value == "3.7200",
            "SellRate con 4 decimales");
        propsCapturadas.Should().Contain(p => p.Name == "UserCode"     && p.Value == "WORKER",
            "UserCode es el usuario del sistema");
    }

    // ── CA-2b: Update NO incluye campos clave (ToCurrCode, FromCurrCode, EffDate) ─

    [Fact]
    public async Task RegistrarTasaCambio_EnUpdate_NoInclueyeCamposClave()
    {
        // Arrange
        _idoMock.Setup(i => i.LoadAsync("SLCurrates", "_ItemId",
                    It.IsAny<string>(), 1, "EffDate DESC", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IdoResponse
                {
                    Success = true,
                    Items   = [new Dictionary<string, string?> { { "_ItemId", ItemIdFicticio } }]
                });

        List<IdoProperty>? propsCapturadas = null;
        _idoMock.Setup(i => i.UpdateItemAsync("SLCurrates", It.IsAny<string>(),
                    It.IsAny<IEnumerable<IdoProperty>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, IEnumerable<IdoProperty>, bool, CancellationToken>(
                    (_, _, props, _, _) => propsCapturadas = props.ToList())
                .ReturnsAsync(new IdoResponse { Success = true });

        var svc = CrearServicio();

        // Act
        await svc.RegistrarTasaCambioAsync(Moneda, Fecha, Compra, Venta, Usuario);

        // Assert — solo se actualizan los valores, no se modifican los campos PK
        propsCapturadas.Should().NotBeNull();
        propsCapturadas.Should().NotContain(p => p.Name == "ToCurrCode",
            "el Update no debe sobrescribir la clave ToCurrCode");
        propsCapturadas.Should().NotContain(p => p.Name == "FromCurrCode",
            "el Update no debe sobrescribir la clave FromCurrCode");
        propsCapturadas.Should().NotContain(p => p.Name == "EffDate",
            "el Update no debe sobrescribir la clave EffDate");
        propsCapturadas.Should().Contain(p => p.Name == "BuyRate",
            "el Update sí debe incluir BuyRate");
        propsCapturadas.Should().Contain(p => p.Name == "SellRate",
            "el Update sí debe incluir SellRate");
        propsCapturadas.Should().Contain(p => p.Name == "UserCode",
            "el Update sí debe incluir UserCode");
    }

    // ── CA-2c: formato de fecha ISO yyyy-MM-dd ────────────────────────────────

    [Theory]
    [InlineData(2026, 1,  5,  "2026-01-05")]
    [InlineData(2026, 12, 31, "2026-12-31")]
    [InlineData(2025, 4,  1,  "2025-04-01")]
    public async Task RegistrarTasaCambio_EffDateFormateadaComoIso8601(int anio, int mes, int dia, string esperado)
    {
        // Arrange
        var fecha = new DateOnly(anio, mes, dia);

        _idoMock.Setup(i => i.LoadAsync("SLCurrates", "_ItemId",
                    It.IsAny<string>(), 1, "EffDate DESC", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IdoResponse { Success = true, Items = [] });

        List<IdoProperty>? propsCapturadas = null;
        _idoMock.Setup(i => i.InsertItemAsync("SLCurrates", It.IsAny<IEnumerable<IdoProperty>>(),
                    It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Callback<string, IEnumerable<IdoProperty>, bool, CancellationToken>(
                    (_, props, _, _) => propsCapturadas = props.ToList())
                .ReturnsAsync(new IdoResponse { Success = true });

        var svc = CrearServicio();

        // Act
        await svc.RegistrarTasaCambioAsync(Moneda, fecha, Compra, Venta, Usuario);

        // Assert
        propsCapturadas.Should().Contain(p => p.Name == "EffDate" && p.Value == esperado,
            $"EffDate debe ser '{esperado}' para fecha {fecha:yyyy-MM-dd}");
    }

    // ── CA-2d: valores numéricos con 4 decimales en cultura InvariantCulture ──

    [Theory]
    [InlineData(3.7,    "3.7000")]
    [InlineData(3.71,   "3.7100")]
    [InlineData(3.7123, "3.7123")]
    [InlineData(4.0,    "4.0000")]
    public async Task RegistrarTasaCambio_BuyRateSellRate_Formateados4Decimales(double valorDouble, string esperado)
    {
        var valor = (decimal)valorDouble;

        // Arrange
        _idoMock.Setup(i => i.LoadAsync("SLCurrates", "_ItemId",
                    It.IsAny<string>(), 1, "EffDate DESC", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IdoResponse { Success = true, Items = [] });

        List<IdoProperty>? propsCapturadas = null;
        _idoMock.Setup(i => i.InsertItemAsync("SLCurrates", It.IsAny<IEnumerable<IdoProperty>>(),
                    It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Callback<string, IEnumerable<IdoProperty>, bool, CancellationToken>(
                    (_, props, _, _) => propsCapturadas = props.ToList())
                .ReturnsAsync(new IdoResponse { Success = true });

        var svc = CrearServicio();

        // Act
        await svc.RegistrarTasaCambioAsync(Moneda, Fecha, valor, valor + 0.01m, Usuario);

        // Assert
        propsCapturadas.Should().Contain(p => p.Name == "BuyRate" && p.Value == esperado,
            $"BuyRate debe ser '{esperado}' para valor {valor}");
    }

    // ── CA-2e: el filtro de búsqueda usa ToCurrCode=PEN y FromCurrCode=moneda ─

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    public async Task RegistrarTasaCambio_FiltroLoadAx_UsaToCurrCodePenYFromCurrCodeCorrecto(string moneda)
    {
        // Arrange
        string? filtroCapturado = null;
        _idoMock.Setup(i => i.LoadAsync("SLCurrates", "_ItemId",
                    It.IsAny<string>(), 1, "EffDate DESC", It.IsAny<CancellationToken>()))
                .Callback<string, string?, string?, int, string?, CancellationToken>(
                    (_, _, f, _, _, _) => filtroCapturado = f)
                .ReturnsAsync(new IdoResponse { Success = true, Items = [] });

        _idoMock.Setup(i => i.InsertItemAsync(It.IsAny<string>(), It.IsAny<IEnumerable<IdoProperty>>(),
                    It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IdoResponse { Success = true });

        var svc = CrearServicio();

        // Act
        await svc.RegistrarTasaCambioAsync(moneda, Fecha, Compra, Venta, Usuario);

        // Assert
        filtroCapturado.Should().NotBeNull();
        filtroCapturado.Should().Contain("ToCurrCode='PEN'",
            "ToCurrCode siempre es PEN (moneda nacional destino)");
        filtroCapturado.Should().Contain($"FromCurrCode='{moneda}'",
            $"FromCurrCode debe ser la moneda de origen ({moneda})");
        filtroCapturado.Should().NotContain("ToCurrCode='USD'",
            "no debe confundir el rol de ToCurrCode y FromCurrCode");
    }

    // ── CA-resilencia: error del IDO → retorna false, no lanza excepción ──────

    [Fact]
    public async Task RegistrarTasaCambio_CuandoIdoLanzaExcepcion_RetornaFalseSinPropagarError()
    {
        // Arrange — simula fallo de conectividad con SyteLine
        _idoMock.Setup(i => i.LoadAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("No se pudo conectar con SyteLine"));

        var svc = CrearServicio();

        // Act
        var ok = await svc.RegistrarTasaCambioAsync(Moneda, Fecha, Compra, Venta, Usuario);

        // Assert
        ok.Should().BeFalse("un fallo en el IDO debe retornar false, no propagar la excepción");
    }
}
