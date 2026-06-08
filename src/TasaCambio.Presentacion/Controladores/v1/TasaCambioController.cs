using MediatR;
using Microsoft.AspNetCore.Mvc;
using TasaCambio.Aplicacion.TasaCambios.Comandos.ActualizarTasaCambio;
using TasaCambio.Aplicacion.TasaCambios.Comandos.CrearTasaCambio;
using TasaCambio.Aplicacion.TasaCambios.Comandos.EliminarTasaCambio;
using TasaCambio.Aplicacion.TasaCambios.Consultas.ListarTasasCambio;
using TasaCambio.Aplicacion.TasaCambios.Consultas.ObtenerTasaCambio;
using TasaCambio.Aplicacion.TasaCambios.Consultas.ObtenerUltimaTasaCambio;

namespace TasaCambio.Presentacion.Controladores.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class TasaCambioController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasaCambioController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{empresa}/{codigoMoneda}")]
    public async Task<IActionResult> Listar(string empresa, string codigoMoneda, [FromQuery] int? anio, [FromQuery] int? mes, CancellationToken ct)
        => Ok(await _mediator.Send(new ListarTasasCambioQuery(empresa, codigoMoneda, anio, mes), ct));

    [HttpGet("{empresa}/{codigoMoneda}/{fecha}")]
    public async Task<IActionResult> ObtenerPorFecha(string empresa, string codigoMoneda, DateOnly fecha, CancellationToken ct)
        => Ok(await _mediator.Send(new ObtenerTasaCambioQuery(empresa, codigoMoneda, fecha), ct));

    [HttpGet("{empresa}/{codigoMoneda}/{fecha}/ultima")]
    public async Task<IActionResult> ObtenerUltima(string empresa, string codigoMoneda, DateOnly fecha, CancellationToken ct)
        => Ok(await _mediator.Send(new ObtenerUltimaTasaCambioQuery(empresa, codigoMoneda, fecha), ct));

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearTasaCambioCommand command, CancellationToken ct)
        => CreatedAtAction(nameof(ObtenerPorFecha), new { command.Empresa, command.CodigoMoneda, command.Fecha },
            await _mediator.Send(command, ct));

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Actualizar(long id, [FromBody] ActualizarTasaCambioCommand command, CancellationToken ct)
        => Ok(await _mediator.Send(command with { Id = id }, ct));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Eliminar(long id, CancellationToken ct)
        => Ok(await _mediator.Send(new EliminarTasaCambioCommand(id), ct));
}
