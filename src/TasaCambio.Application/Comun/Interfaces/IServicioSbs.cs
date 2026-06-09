using TasaCambio.Application.Sbs;

namespace TasaCambio.Application.Comun.Interfaces;

public interface IServicioSbs
{
    Task<SbsTasaCambioDto?> ObtenerTasaCambioAsync(string codigoMoneda, DateOnly fecha, CancellationToken ct = default);
}
