using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TasaCambio.Application.Comun.Dtos;
using TasaCambio.Application.TasaCambios;
using TasaCambio.Application.TasaCambios.Consultas.ListarTasasCambio;
using TasaCambio.Application.TasaCambios.Consultas.ObtenerTasaCambio;
using TasaCambio.Application.TasaCambios.Consultas.ObtenerUltimaTasaCambio;

namespace TasaCambio.Presentation.Controladores.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class TipoCambioController : ControllerBase
{
    private readonly IMediator _mediator;

    public TipoCambioController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{codigoMoneda}")]
    [ProducesResponseType(typeof(ResponseDto<IReadOnlyList<TasaCambioDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(string codigoMoneda, [FromQuery] int? anio, [FromQuery] int? mes, CancellationToken ct)
        => Ok(await _mediator.Send(new ListarTasasCambioQuery(codigoMoneda, anio, mes), ct));

    [HttpGet("{codigoMoneda}/{fecha}")]
    [ProducesResponseType(typeof(ResponseDto<TasaCambioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorFecha(string codigoMoneda, DateOnly fecha, CancellationToken ct)
        => Ok(await _mediator.Send(new ObtenerTasaCambioQuery(codigoMoneda, fecha), ct));

    [HttpGet("{codigoMoneda}/{fecha}/ultima")]
    [ProducesResponseType(typeof(ResponseDto<TasaCambioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerUltima(string codigoMoneda, DateOnly fecha, CancellationToken ct)
        => Ok(await _mediator.Send(new ObtenerUltimaTasaCambioQuery(codigoMoneda, fecha), ct));
}
