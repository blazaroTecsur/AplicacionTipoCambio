using FluentValidation;

namespace TasaCambio.Aplicacion.TasaCambios.Comandos.CrearTasaCambio;

public sealed class CrearTasaCambioValidator : AbstractValidator<CrearTasaCambioCommand>
{
    public CrearTasaCambioValidator()
    {
        RuleFor(x => x.Empresa).NotEmpty().MaximumLength(20);
        RuleFor(x => x.CodigoMoneda).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Fecha).NotEmpty();
        RuleFor(x => x.ValorCompra).GreaterThan(0).WithMessage("El valor de compra debe ser mayor a cero.");
        RuleFor(x => x.ValorVenta).GreaterThan(0).WithMessage("El valor de venta debe ser mayor a cero.");
        RuleFor(x => x).Must(x => x.ValorCompra <= x.ValorVenta)
            .WithMessage("El valor de compra no puede ser mayor al de venta.");
    }
}
