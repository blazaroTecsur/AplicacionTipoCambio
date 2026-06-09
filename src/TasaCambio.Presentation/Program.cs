using System.Threading.RateLimiting;
using Serilog;
using TasaCambio.Application;
using TasaCambio.Application.Comun.Interfaces;
using TasaCambio.Infrastructure;
using TasaCambio.Presentation.Extensiones;
using TasaCambio.Presentation.Middleware;
using TasaCambio.Worker;
using TasaCambio.Worker.Configuracion;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq(ctx.Configuration["Serilog:SeqUrl"] ?? "http://localhost:5341"));

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AgregarSwagger();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IContextoUsuario, ContextoUsuarioHttp>();
    builder.Services.AddTransient<ManejadorExcepciones>();
    builder.Services.AddTransient<AutenticacionApiKey>();

    builder.Services.AgregarAplicacion();
    builder.Services.AgregarInfraestructura(builder.Configuration);

    var sbsConfig = builder.Configuration.GetSection("Sbs").Get<SbsWorkerConfig>() ?? new SbsWorkerConfig();
    builder.Services.AddSingleton(sbsConfig);
    builder.Services.AddHostedService<SbsSyncWorker>();

    builder.Services.AddRateLimiter(options =>
    {
        options.AddPolicy("fixed", context => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
    });

    builder.Services.AddHealthChecks();

    var app = builder.Build();

    app.UseMiddleware<ManejadorExcepciones>();
    app.UseMiddleware<AutenticacionApiKey>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseRateLimiter();
    app.MapControllers().RequireRateLimiting("fixed");
    app.MapHealthChecks("/health");
    app.UseSerilogRequestLogging();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente.");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
