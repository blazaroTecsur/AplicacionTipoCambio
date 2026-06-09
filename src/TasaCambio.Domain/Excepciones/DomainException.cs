namespace TasaCambio.Domain.Excepciones;

public sealed class DomainException : Exception
{
    public DomainException(string mensaje) : base(mensaje) { }
}
