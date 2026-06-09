namespace TasaCambio.Worker.Configuracion;

public sealed class SbsWorkerConfig
{
    public int HoraInicioRegistro { get; init; } = 21;
    public int HoraFinRegistro { get; init; } = 6;
    public int IntervaloBusquedaMinutos { get; init; } = 30;
    public int ValidacionPartesEnteras { get; init; } = 2;
    public int ValidacionPartesDecimales { get; init; } = 6;
    public List<TrabajoSbs> Trabajos { get; init; } = [];

    public bool EstaEnVentanaActualizacion()
    {
        var horaActual = DateTime.Now.Hour;
        return horaActual >= HoraInicioRegistro || horaActual <= HoraFinRegistro;
    }
}

public sealed class TrabajoSbs
{
    public string Empresa { get; init; } = string.Empty;
    public string CodigoMoneda { get; init; } = string.Empty;
}
