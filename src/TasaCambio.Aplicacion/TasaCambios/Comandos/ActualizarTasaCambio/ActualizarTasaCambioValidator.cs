using FluentValidation;

namespace TasaCambio.Aplicacion.TasaCambios.Comandos.ActualizarTasaCambio;

public sealed class ActualizarTasaCambioValidator : AbstractValidator<ActualizarTasaCambioCommand>
{
    public ActualizarTasaCambioValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.ValorCompra).GreaterThan(0);
        RuleFor(x => x.ValorVenta).GreaterThan(0);
        RuleFor(x => x).Must(x => x.ValorCompra <= x.ValorVenta)
            .WithMessage("El valor de compra no puede ser mayor al de venta.");
    }
}
