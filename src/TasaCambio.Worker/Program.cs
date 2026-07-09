using Serilog;
using TasaCambio.Application;
using TasaCambio.Application.Comun.Interfaces;
using TasaCambio.Infrastructure;
using TasaCambio.Worker;
using TasaCambio.Worker.Configuracion;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq(builder.Configuration["Serilog:SeqUrl"] ?? "http://localhost:5341"));

    var sbsConfig = builder.Configuration.GetSection("Sbs").Get<SbsWorkerConfig>() ?? new SbsWorkerConfig();
    builder.Services.AddSingleton(sbsConfig);

    builder.Services.AgregarAplicacion();
    builder.Services.AgregarInfraestructura(builder.Configuration);
    builder.Services.AddScoped<IContextoUsuario, ContextoUsuarioSistema>();

    builder.Services.AddHostedService<SbsSyncWorker>();

    var host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker terminó inesperadamente.");
}
finally
{
    Log.CloseAndFlush();
}
