using Microsoft.Extensions.Logging;
using TasaCambio.Application.Comun.Dtos;
using TasaCambio.Application.Comun.Interfaces;
using TasaCambio.Application.Sbs;
using TasaCambio.Application.TasaCambios.Comandos.SincronizarDesdeSbs;
using TasaCambio.Domain.Excepciones;
using TasaCambio.Domain.Interfaces;

namespace TasaCambio.UnitTest.HU01_ObtencionAutomaticaSbs;

/// <summary>
/// Pruebas unitarias para HU-01: Obtención automática del tipo de cambio desde la SBS.
/// Cubre los criterios de aceptación del handler SincronizarDesdeSbsHandler.
/// </summary>
public class SincronizarDesdeSbsHandlerTests
{
    // ── Mocks ─────────────────────────────────────────────────────────────────
    private readonly Mock<IServicioSbs>           _sbsMock       = new();
    private readonly Mock<IServicioSyteline>      _syteMock      = new();
    private readonly Mock<IUnidadDeTrabajo>       _uowMock       = new();
    private readonly Mock<IContextoUsuario>       _ctxMock       = new();
    private readonly Mock<IServicioAuditoria>     _auditoriaMock = new();
    private readonly Mock<ITasaCambioRepositorio> _repoMock      = new();
    // NullLogger evita el problema de Castle DynamicProxy con tipos internos
    private readonly ILogger<SincronizarDesdeSbsHandler> _logger =
        NullLogger<SincronizarDesdeSbsHandler>.Instance;

    // ── Datos de prueba comunes ───────────────────────────────────────────────
    private static readonly DateOnly FechaRequest = new(2026, 7, 17);
    private static readonly DateOnly FechaSbs     = new(2026, 7, 14); // SBS publica el día anterior

    private SincronizarDesdeSbsHandler CrearHandler()
    {
        _uowMock.Setup(u => u.TasaCambios).Returns(_repoMock.Object);
        _ctxMock.Setup(c => c.NombreUsuario).Returns("WORKER");

        // Defaults seguros para dependencias no críticas en cada test
        _auditoriaMock
            .Setup(a => a.RegistrarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
        _syteMock
            .Setup(s => s.RegistrarTasaCambioAsync(It.IsAny<string>(), It.IsAny<DateOnly>(),
                It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        return new SincronizarDesdeSbsHandler(
            _sbsMock.Object,
            _syteMock.Object,
            _uowMock.Object,
            _ctxMock.Object,
            _auditoriaMock.Object,
            _logger);
    }

    private static SbsTasaCambioDto CrearSbsDto(
        string moneda  = "USD",
        string compra  = "3.7100",
        string venta   = "3.7200",
        DateOnly? fecha = null)
        => new()
        {
            CodigoMoneda      = moneda,
            DescripcionMoneda = "Dolar Americano",
            ValorCompra       = compra,
            ValorVenta        = venta,
            Fecha             = fecha ?? FechaSbs,
        };

    // ── CA-1: SBS retorna null → NotFoundException ────────────────────────────

    [Fact]
    public async Task Handle_CuandoSbsNoPublicaMoneda_LanzaNotFoundException()
    {
        // Arrange
        _sbsMock.Setup(s => s.ObtenerTasaCambioAsync("USD", FechaRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync((SbsTasaCambioDto?)null);

        var handler = CrearHandler();
        var command = new SincronizarDesdeSbsCommand("USD", FechaRequest);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));

        ex.Message.Should().Contain("USD");
    }

    // ── CA-2 / CA-nueva entrada: no existe registro previo → crea en BD ───────

    [Fact]
    public async Task Handle_CuandoNoExisteRegistroPrevio_CreaEntradaEnBdInterna()
    {
        // Arrange
        _sbsMock.Setup(s => s.ObtenerTasaCambioAsync("USD", FechaRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CrearSbsDto());

        _repoMock.Setup(r => r.ObtenerPorFechaAsync("USD", FechaSbs, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((TasaCambioEntity?)null);

        TasaCambioEntity? entidadGuardada = null;
        _repoMock.Setup(r => r.AgregarAsync(It.IsAny<TasaCambioEntity>(), It.IsAny<CancellationToken>()))
                 .Callback<TasaCambioEntity, CancellationToken>((e, _) => entidadGuardada = e)
                 .ReturnsAsync(TasaCambioEntity.Crear("USD", FechaSbs, 3.71m, 3.72m, "WORKER", "SBS"));

        var handler = CrearHandler();

        // Act
        var result = await handler.Handle(new SincronizarDesdeSbsCommand("USD", FechaRequest), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();

        _repoMock.Verify(r => r.AgregarAsync(It.IsAny<TasaCambioEntity>(), It.IsAny<CancellationToken>()), Times.Once,
            "debe insertar una nueva entidad en la BD interna");
        _uowMock.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once,
            "debe persistir los cambios");

        entidadGuardada.Should().NotBeNull();
        entidadGuardada!.CodigoMoneda.Should().Be("USD");
        entidadGuardada.ValorCompra.Should().Be(3.71m);
        entidadGuardada.ValorVenta.Should().Be(3.72m);
    }

    // ── CA-3: ya existe con mismos valores → no actualiza BD ─────────────────

    [Fact]
    public async Task Handle_CuandoExisteConMismosValores_NoActualizaBdInterna()
    {
        // Arrange — los valores son idénticos, no debe tocar la BD
        _sbsMock.Setup(s => s.ObtenerTasaCambioAsync("USD", FechaRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CrearSbsDto(compra: "3.7100", venta: "3.7200"));

        var existente = TasaCambioEntity.Crear("USD", FechaSbs, 3.71m, 3.72m, "WORKER", "SBS");
        _repoMock.Setup(r => r.ObtenerPorFechaAsync("USD", FechaSbs, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existente);

        var handler = CrearHandler();

        // Act
        var result = await handler.Handle(new SincronizarDesdeSbsCommand("USD", FechaRequest), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("ya estaba actualizada");

        _repoMock.Verify(r => r.ActualizarAsync(It.IsAny<TasaCambioEntity>(), It.IsAny<CancellationToken>()), Times.Never,
            "no debe actualizar si los valores no cambiaron");
        _uowMock.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never,
            "no debe persistir si no hubo cambio");
    }

    // ── CA-4: ya existe con valores distintos → actualiza BD ─────────────────

    [Fact]
    public async Task Handle_CuandoExisteConValoresDiferentes_ActualizaBdInterna()
    {
        // Arrange — SBS publicó nuevos valores
        _sbsMock.Setup(s => s.ObtenerTasaCambioAsync("USD", FechaRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CrearSbsDto(compra: "3.7100", venta: "3.7200"));

        var existente = TasaCambioEntity.Crear("USD", FechaSbs, 3.69m, 3.70m, "WORKER", "SBS"); // valores anteriores
        _repoMock.Setup(r => r.ObtenerPorFechaAsync("USD", FechaSbs, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existente);
        _repoMock.Setup(r => r.ActualizarAsync(It.IsAny<TasaCambioEntity>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var handler = CrearHandler();

        // Act
        var result = await handler.Handle(new SincronizarDesdeSbsCommand("USD", FechaRequest), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("actualizada");

        _repoMock.Verify(r => r.ActualizarAsync(It.IsAny<TasaCambioEntity>(), It.IsAny<CancellationToken>()), Times.Once,
            "debe actualizar la entidad en BD con los nuevos valores");
        _uowMock.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);

        existente.ValorCompra.Should().Be(3.71m, "ActualizarValores debe haber actualizado la compra");
        existente.ValorVenta.Should().Be(3.72m,  "ActualizarValores debe haber actualizado la venta");
    }

    // ── CA-5: la fecha autorativa es la publicada por SBS ────────────────────

    [Fact]
    public async Task Handle_UsaFechaPublicadaPorSbs_NoFechaDelRequest()
    {
        // FechaRequest = 17/07/2026 (hoy) pero SBS solo publicó hasta 14/07/2026
        _sbsMock.Setup(s => s.ObtenerTasaCambioAsync("USD", FechaRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CrearSbsDto(fecha: FechaSbs)); // <-- fecha en respuesta SBS

        DateOnly fechaConsultadaEnRepo = default;
        _repoMock.Setup(r => r.ObtenerPorFechaAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                 .Callback<string, DateOnly, CancellationToken>((_, f, _) => fechaConsultadaEnRepo = f)
                 .ReturnsAsync((TasaCambioEntity?)null);

        TasaCambioEntity? entidadCreada = null;
        _repoMock.Setup(r => r.AgregarAsync(It.IsAny<TasaCambioEntity>(), It.IsAny<CancellationToken>()))
                 .Callback<TasaCambioEntity, CancellationToken>((e, _) => entidadCreada = e)
                 .ReturnsAsync(TasaCambioEntity.Crear("USD", FechaSbs, 3.71m, 3.72m, "WORKER", "SBS"));

        var handler = CrearHandler();

        // Act
        await handler.Handle(new SincronizarDesdeSbsCommand("USD", FechaRequest), CancellationToken.None);

        // Assert
        fechaConsultadaEnRepo.Should().Be(FechaSbs,
            "el handler debe buscar en la BD usando la fecha publicada por SBS (14/07), no la del request (17/07)");

        entidadCreada!.Fecha.Should().Be(FechaSbs,
            "la entidad creada debe tener la fecha de la SBS");
    }

    // ── CA-6: SyteLine retorna false → la operación sigue siendo exitosa ──────

    [Fact]
    public async Task Handle_CuandoSytelineRetornaFalse_OperacionSigueSiendoExitosa()
    {
        // Arrange
        _sbsMock.Setup(s => s.ObtenerTasaCambioAsync("USD", FechaRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CrearSbsDto());
        _repoMock.Setup(r => r.ObtenerPorFechaAsync("USD", FechaSbs, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((TasaCambioEntity?)null);
        _repoMock.Setup(r => r.AgregarAsync(It.IsAny<TasaCambioEntity>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(TasaCambioEntity.Crear("USD", FechaSbs, 3.71m, 3.72m, "WORKER", "SBS"));

        // SyteLine falla silenciosamente
        _syteMock.Setup(s => s.RegistrarTasaCambioAsync(It.IsAny<string>(), It.IsAny<DateOnly>(),
                It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);

        var handler = CrearHandler();

        // Act
        var result = await handler.Handle(new SincronizarDesdeSbsCommand("USD", FechaRequest), CancellationToken.None);

        // Assert — la BD interna está bien; SyteLine es "mejor esfuerzo"
        result.Success.Should().BeTrue(
            "un fallo en SyteLine no debe cancelar la operación; el dato ya está en la BD interna");
    }

    // ── CA-6b: SyteLine lanza excepción → la operación sigue siendo exitosa ──

    [Fact]
    public async Task Handle_CuandoSytelineLanzaExcepcion_OperacionNoSePropagaError()
    {
        // Arrange
        _sbsMock.Setup(s => s.ObtenerTasaCambioAsync("USD", FechaRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CrearSbsDto());
        _repoMock.Setup(r => r.ObtenerPorFechaAsync("USD", FechaSbs, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((TasaCambioEntity?)null);
        _repoMock.Setup(r => r.AgregarAsync(It.IsAny<TasaCambioEntity>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(TasaCambioEntity.Crear("USD", FechaSbs, 3.71m, 3.72m, "WORKER", "SBS"));

        _syteMock.Setup(s => s.RegistrarTasaCambioAsync(It.IsAny<string>(), It.IsAny<DateOnly>(),
                It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ThrowsAsync(new HttpRequestException("SyteLine no disponible"));

        var handler = CrearHandler();

        // Act
        var result = await handler.Handle(new SincronizarDesdeSbsCommand("USD", FechaRequest), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue(
            "una excepción en SyteLine está atrapada internamente; la operación no debe fallar");
    }

    // ── CA-extra: también llama a SyteLine cuando crea o actualiza ────────────

    [Fact]
    public async Task Handle_SiempreLlamaSytelineParaSincronizar_AunqueNoHubieraCambioEnBd()
    {
        // Mismos valores → no actualiza BD pero sí debe llamar SyteLine
        _sbsMock.Setup(s => s.ObtenerTasaCambioAsync("USD", FechaRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CrearSbsDto(compra: "3.7100", venta: "3.7200"));

        var existente = TasaCambioEntity.Crear("USD", FechaSbs, 3.71m, 3.72m, "WORKER", "SBS");
        _repoMock.Setup(r => r.ObtenerPorFechaAsync("USD", FechaSbs, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existente);

        var handler = CrearHandler();

        // Act
        await handler.Handle(new SincronizarDesdeSbsCommand("USD", FechaRequest), CancellationToken.None);

        // Assert
        _syteMock.Verify(s => s.RegistrarTasaCambioAsync("USD", FechaSbs, 3.71m, 3.72m, "WORKER", It.IsAny<CancellationToken>()),
            Times.Once, "debe sincronizar con SyteLine siempre que SBS responda correctamente");
    }
}
