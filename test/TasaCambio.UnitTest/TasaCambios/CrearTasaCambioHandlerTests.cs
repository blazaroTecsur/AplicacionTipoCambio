using FluentAssertions;
using MediatR;
using Moq;
using TasaCambio.Application.Comun.Interfaces;
using TasaCambio.Application.TasaCambios.Comandos.CrearTasaCambio;
using TasaCambio.Domain.Interfaces;

namespace TasaCambio.UnitTest.TasaCambios;

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
        _repoMock.Setup(r => r.AgregarAsync(It.IsAny<Domain.Entidades.TasaCambio>(), default))
            .ReturnsAsync((Domain.Entidades.TasaCambio t, CancellationToken _) => t);
        _auditoriaMock.Setup(a => a.RegistrarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ConDatosValidos_RetornaExitoso()
    {
        var handler = (IRequestHandler<CrearTasaCambioCommand, Application.Comun.Dtos.ResponseDto<Application.TasaCambios.TasaCambioDto>>)
            Activator.CreateInstance(
                typeof(CrearTasaCambioCommand).Assembly.GetType("TasaCambio.Application.TasaCambios.Comandos.CrearTasaCambio.CrearTasaCambioHandler")!,
                _uowMock.Object, _contextoMock.Object, _auditoriaMock.Object)!;

        var command = new CrearTasaCambioCommand("EMP01", "USD", DateOnly.FromDateTime(DateTime.Today), 3.70m, 3.80m);

        var resultado = await handler.Handle(command, default);

        resultado.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.CodigoMoneda.Should().Be("USD");
    }

    [Fact]
    public void Crear_ConComprasMayorVenta_LanzaDomainException()
    {
        var accion = () => Domain.Entidades.TasaCambio.Crear("EMP01", "USD", DateOnly.FromDateTime(DateTime.Today), 3.90m, 3.80m, "user");
        accion.Should().Throw<Domain.Excepciones.DomainException>();
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
