namespace TasaCambio.Domain.Excepciones;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string entidad, object llave)
        : base($"{entidad} con identificador '{llave}' no fue encontrado.") { }
}
