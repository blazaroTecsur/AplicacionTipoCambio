using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TasaCambio.Application.Comun.Dtos;
using TasaCambio.Application.TasaCambios;
using TasaCambio.Application.TasaCambios.Comandos.ActualizarTasaCambio;
using TasaCambio.Application.TasaCambios.Comandos.CrearTasaCambio;
using TasaCambio.Application.TasaCambios.Comandos.EliminarTasaCambio;
using TasaCambio.Application.TasaCambios.Comandos.SincronizarDesdeSbs;
using TasaCambio.Application.TasaCambios.Consultas.ListarTasasCambio;
using TasaCambio.Application.TasaCambios.Consultas.ObtenerTasaCambio;
using TasaCambio.Application.TasaCambios.Consultas.ObtenerUltimaTasaCambio;

namespace TasaCambio.Presentation.Controladores.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class TasaCambioController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasaCambioController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{empresa}/{codigoMoneda}")]
    [ProducesResponseType(typeof(ResponseDto<IReadOnlyList<TasaCambioDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(string empresa, string codigoMoneda, [FromQuery] int? anio, [FromQuery] int? mes, CancellationToken ct)
        => Ok(await _mediator.Send(new ListarTasasCambioQuery(empresa, codigoMoneda, anio, mes), ct));

    [HttpGet("{empresa}/{codigoMoneda}/{fecha}")]
    [ProducesResponseType(typeof(ResponseDto<TasaCambioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorFecha(string empresa, string codigoMoneda, DateOnly fecha, CancellationToken ct)
        => Ok(await _mediator.Send(new ObtenerTasaCambioQuery(empresa, codigoMoneda, fecha), ct));

    [HttpGet("{empresa}/{codigoMoneda}/{fecha}/ultima")]
    [ProducesResponseType(typeof(ResponseDto<TasaCambioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerUltima(string empresa, string codigoMoneda, DateOnly fecha, CancellationToken ct)
        => Ok(await _mediator.Send(new ObtenerUltimaTasaCambioQuery(empresa, codigoMoneda, fecha), ct));

    [HttpPost]
    [ProducesResponseType(typeof(ResponseDto<TasaCambioDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearTasaCambioCommand command, CancellationToken ct)
        => CreatedAtAction(nameof(ObtenerPorFecha), new { command.Empresa, command.CodigoMoneda, command.Fecha },
            await _mediator.Send(command, ct));

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ResponseDto<TasaCambioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(long id, [FromBody] ActualizarTasaCambioCommand command, CancellationToken ct)
        => Ok(await _mediator.Send(command with { Id = id }, ct));

    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(ResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(long id, CancellationToken ct)
        => Ok(await _mediator.Send(new EliminarTasaCambioCommand(id), ct));

    [HttpPost("sincronizar-sbs")]
    [ProducesResponseType(typeof(ResponseDto<TasaCambioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SincronizarDesdeSbs([FromBody] SincronizarDesdeSbsCommand command, CancellationToken ct)
        => Ok(await _mediator.Send(command, ct));
}
