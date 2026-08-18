namespace TasaCambio.Application.Comun.Interfaces;

public interface IServicioSyteline
{
    Task<bool> RegistrarTasaCambioAsync(
        string codigoMoneda,
        DateOnly fecha,
        decimal compra,
        decimal venta,
        string usuario,
        CancellationToken ct = default);
}
