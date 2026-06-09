namespace TasaCambio.Worker.Configuracion;

public sealed class SbsWorkerConfig
{
    public string HoraEjecucion { get; init; } = "09:00";
    public int ValidacionPartesEnteras { get; init; } = 2;
    public int ValidacionPartesDecimales { get; init; } = 6;
    public List<TrabajoSbs> Trabajos { get; init; } = [];
}

public sealed class TrabajoSbs
{
    public string Empresa { get; init; } = string.Empty;
    public string CodigoMoneda { get; init; } = string.Empty;
}
