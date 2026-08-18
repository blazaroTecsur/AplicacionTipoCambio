namespace TasaCambio.Worker.Configuracion;

public sealed class SbsWorkerConfig
{
    public int HoraInicioRegistro { get; init; } = 21;
    public int HoraFinRegistro { get; init; } = 6;
    public int IntervaloBusquedaMinutos { get; init; } = 30;
    public int ValidacionPartesEnteras { get; init; } = 2;
    public int ValidacionPartesDecimales { get; init; } = 6;
    public List<string> Monedas { get; init; } = [];

    public bool EstaEnVentanaActualizacion()
    {
        var horaActual = DateTime.Now.Hour;

        // Ventana nocturna (cruza medianoche): ej. 21-6 → hora >= 21 OR hora <= 6
        if (HoraInicioRegistro > HoraFinRegistro)
            return horaActual >= HoraInicioRegistro || horaActual <= HoraFinRegistro;

        // Ventana diurna (mismo día): ej. 8-9 → hora >= 8 AND hora <= 9
        return horaActual >= HoraInicioRegistro && horaActual <= HoraFinRegistro;
    }
}
