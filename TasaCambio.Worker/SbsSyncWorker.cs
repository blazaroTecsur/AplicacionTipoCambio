using MediatR;
using TasaCambio.Application.TasaCambios.Comandos.SincronizarDesdeSbs;
using TasaCambio.Worker.Configuracion;

namespace TasaCambio.Worker;

public class SbsSyncWorker : BackgroundService
{
    private readonly ILogger<SbsSyncWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SbsWorkerConfig _config;

    public SbsSyncWorker(
        ILogger<SbsSyncWorker> logger,
        IServiceScopeFactory scopeFactory,
        SbsWorkerConfig config)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation(
            "Worker SBS iniciado. Ventana de actualización: {Inicio}:00 - {Fin}:00. Intervalo: {Intervalo} min.",
            _config.HoraInicioRegistro, _config.HoraFinRegistro, _config.IntervaloBusquedaMinutos);

        while (!ct.IsCancellationRequested)
        {
            if (_config.EstaEnVentanaActualizacion())
            {
                _logger.LogInformation("En ventana de ACTUALIZACIÓN. Sincronizando con SBS...");
                await EjecutarActualizacionAsync(ct);
            }
            else
            {
                _logger.LogInformation(
                    "Fuera de ventana de actualización (hora actual: {Hora}:00). Solo validación, no se graba en BD.",
                    DateTime.Now.Hour);
                await EjecutarValidacionAsync(ct);
            }

            await Task.Delay(TimeSpan.FromMinutes(_config.IntervaloBusquedaMinutos), ct);
        }
    }

    private async Task EjecutarActualizacionAsync(CancellationToken ct)
    {
        var fecha = DateOnly.FromDateTime(DateTime.Today);

        foreach (var codigoMoneda in _config.Monedas)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var comando = new SincronizarDesdeSbsCommand(codigoMoneda, fecha);
                var resultado = await mediator.Send(comando, ct);

                if (resultado.Success)
                    _logger.LogInformation(
                        "[ACTUALIZACIÓN] {Moneda} - Compra: {Compra} / Venta: {Venta}",
                        codigoMoneda,
                        resultado.Data?.ValorCompra, resultado.Data?.ValorVenta);
                else
                    _logger.LogWarning(
                        "[ACTUALIZACIÓN] {Moneda} - {Errores}",
                        codigoMoneda, string.Join(", ", resultado.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ACTUALIZACIÓN] Error en {Moneda}", codigoMoneda);
            }
        }
    }

    private async Task EjecutarValidacionAsync(CancellationToken ct)
    {
        var fecha = DateOnly.FromDateTime(DateTime.Today);

        foreach (var codigoMoneda in _config.Monedas)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var servicioSbs = scope.ServiceProvider.GetRequiredService<Application.Comun.Interfaces.IServicioSbs>();

                var resultado = await servicioSbs.ObtenerTasaCambioAsync(codigoMoneda, fecha, ct);

                if (resultado is not null)
                    _logger.LogInformation(
                        "[VALIDACIÓN] {Moneda} - Compra: {Compra} / Venta: {Venta} (no se guarda en BD)",
                        codigoMoneda,
                        resultado.ValorCompra, resultado.ValorVenta);
                else
                    _logger.LogWarning(
                        "[VALIDACIÓN] {Moneda} - Sin datos en SBS para {Fecha}",
                        codigoMoneda, fecha);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VALIDACIÓN] Error en {Moneda}", codigoMoneda);
            }
        }
    }
}
