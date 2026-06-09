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
        _logger.LogInformation("Worker SBS iniciado.");

        while (!ct.IsCancellationRequested)
        {
            var ahora = DateTime.Now;
            var horaEjecucion = TimeOnly.Parse(_config.HoraEjecucion);
            var proximaEjecucion = ahora.Date.Add(horaEjecucion.ToTimeSpan());

            if (proximaEjecucion <= ahora)
                proximaEjecucion = proximaEjecucion.AddDays(1);

            var espera = proximaEjecucion - ahora;
            _logger.LogInformation("Próxima ejecución: {ProximaEjecucion}", proximaEjecucion);

            await Task.Delay(espera, ct);

            await EjecutarSincronizacionAsync(ct);
        }
    }

    private async Task EjecutarSincronizacionAsync(CancellationToken ct)
    {
        var fecha = DateOnly.FromDateTime(DateTime.Today);
        _logger.LogInformation("Iniciando sincronización SBS para fecha {Fecha}", fecha);

        foreach (var trabajo in _config.Trabajos)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var comando = new SincronizarDesdeSbsCommand(trabajo.Empresa, trabajo.CodigoMoneda, fecha);
                var resultado = await mediator.Send(comando, ct);

                if (resultado.Success)
                    _logger.LogInformation("Sincronizado: {Empresa}/{Moneda} = Compra:{Compra} Venta:{Venta}",
                        trabajo.Empresa, trabajo.CodigoMoneda,
                        resultado.Data?.ValorCompra, resultado.Data?.ValorVenta);
                else
                    _logger.LogWarning("Sin datos: {Empresa}/{Moneda} - {Errores}",
                        trabajo.Empresa, trabajo.CodigoMoneda, string.Join(", ", resultado.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sincronizando {Empresa}/{Moneda}", trabajo.Empresa, trabajo.CodigoMoneda);
            }
        }

        _logger.LogInformation("Sincronización SBS completada.");
    }
}
