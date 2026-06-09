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

        foreach (var trabajo in _config.Trabajos)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var comando = new SincronizarDesdeSbsCommand(trabajo.Empresa, trabajo.CodigoMoneda, fecha);
                var resultado = await mediator.Send(comando, ct);

                if (resultado.Success)
                    _logger.LogInformation(
                        "[ACTUALIZACIÓN] {Empresa}/{Moneda} - Compra: {Compra} / Venta: {Venta}",
                        trabajo.Empresa, trabajo.CodigoMoneda,
                        resultado.Data?.ValorCompra, resultado.Data?.ValorVenta);
                else
                    _logger.LogWarning(
                        "[ACTUALIZACIÓN] {Empresa}/{Moneda} - {Errores}",
                        trabajo.Empresa, trabajo.CodigoMoneda, string.Join(", ", resultado.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ACTUALIZACIÓN] Error en {Empresa}/{Moneda}", trabajo.Empresa, trabajo.CodigoMoneda);
            }
        }
    }

    private async Task EjecutarValidacionAsync(CancellationToken ct)
    {
        var fecha = DateOnly.FromDateTime(DateTime.Today);

        foreach (var trabajo in _config.Trabajos)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var servicioSbs = scope.ServiceProvider.GetRequiredService<Application.Comun.Interfaces.IServicioSbs>();

                var resultado = await servicioSbs.ObtenerTasaCambioAsync(trabajo.CodigoMoneda, fecha, ct);

                if (resultado is not null)
                    _logger.LogInformation(
                        "[VALIDACIÓN] {Empresa}/{Moneda} - Compra: {Compra} / Venta: {Venta} (no se guarda en BD)",
                        trabajo.Empresa, trabajo.CodigoMoneda,
                        resultado.ValorCompra, resultado.ValorVenta);
                else
                    _logger.LogWarning(
                        "[VALIDACIÓN] {Empresa}/{Moneda} - Sin datos en SBS para {Fecha}",
                        trabajo.Empresa, trabajo.CodigoMoneda, fecha);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VALIDACIÓN] Error en {Empresa}/{Moneda}", trabajo.Empresa, trabajo.CodigoMoneda);
            }
        }
    }
}
