using FluentAssertions;
using MediatR;
using Moq;
using TasaCambio.Aplicacion.Comun.Interfaces;
using TasaCambio.Aplicacion.TasaCambios.Comandos.CrearTasaCambio;
using TasaCambio.Dominio.Interfaces;

namespace TasaCambio.PruebaUnitaria.TasaCambios;

public sealed class CrearTasaCambioHandlerTests
{
    private readonly Mock<IUnidadDeTrabajo> _uowMock = new();
    private readonly Mock<IContextoUsuario> _contextoMock = new();
    private readonly Mock<IServicioAuditoria> _auditoriaMock = new();
    private readonly Mock<ITasaCambioRepositorio> _repoMock = new();

    public CrearTasaCambioHandlerTests()
    {
        _contextoMock.Setup(c => c.NombreUsuario).Returns("test-user");
        _uowMock.Setup(u => u.TasaCambios).Returns(_repoMock.Object);
        _uowMock.Setup(u => u.GuardarCambiosAsync(default)).ReturnsAsync(1);
        _repoMock.Setup(r => r.AgregarAsync(It.IsAny<Dominio.Entidades.TasaCambio>(), default))
            .ReturnsAsync((Dominio.Entidades.TasaCambio t, CancellationToken _) => t);
        _auditoriaMock.Setup(a => a.RegistrarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ConDatosValidos_RetornaExitoso()
    {
        var handler = (IRequestHandler<CrearTasaCambioCommand, Aplicacion.Comun.Dtos.RespuestaDto<Aplicacion.TasaCambios.TasaCambioDto>>)
            Activator.CreateInstance(
                typeof(CrearTasaCambioCommand).Assembly.GetType("TasaCambio.Aplicacion.TasaCambios.Comandos.CrearTasaCambio.CrearTasaCambioHandler")!,
                _uowMock.Object, _contextoMock.Object, _auditoriaMock.Object)!;

        var command = new CrearTasaCambioCommand("EMP01", "USD", DateOnly.FromDateTime(DateTime.Today), 3.70m, 3.80m);

        var resultado = await handler.Handle(command, default);

        resultado.Exitoso.Should().BeTrue();
        resultado.Datos.Should().NotBeNull();
        resultado.Datos!.CodigoMoneda.Should().Be("USD");
    }

    [Fact]
    public void Crear_ConComprasMayorVenta_LanzaDomainException()
    {
        var accion = () => Dominio.Entidades.TasaCambio.Crear("EMP01", "USD", DateOnly.FromDateTime(DateTime.Today), 3.90m, 3.80m, "user");
        accion.Should().Throw<Dominio.Excepciones.DomainException>();
    }

    [Fact]
    public async Task Validator_ConEmpresaVacia_RetornaErrores()
    {
        var validator = new CrearTasaCambioValidator();
        var command = new CrearTasaCambioCommand("", "USD", DateOnly.FromDateTime(DateTime.Today), 3.70m, 3.80m);

        var resultado = await validator.ValidateAsync(command);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Empresa");
    }
}
