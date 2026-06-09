using MediatR;
using Microsoft.AspNetCore.Mvc;
using TasaCambio.Application.Comun.Dtos;
using TasaCambio.Application.Monedas;
using TasaCambio.Application.Monedas.Consultas.ListarMonedas;

namespace TasaCambio.Presentation.Controladores.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class MonedaController : ControllerBase
{
    private readonly IMediator _mediator;

    public MonedaController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{empresa}")]
    [ProducesResponseType(typeof(ResponseDto<IReadOnlyList<MonedaDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(string empresa, CancellationToken ct)
        => Ok(await _mediator.Send(new ListarMonedasQuery(empresa), ct));
}
