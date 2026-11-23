using GesCredito.Domain.Enums;
using GesCredito.Services.Dtos;
using GesCredito.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GesCredito.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreditosController(ICreditoService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<CreditoDto>>> Listar(
        [FromQuery] int? clienteId, [FromQuery] EstadoCredito? estado,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default) =>
        Ok(await service.ListarAsync(clienteId, estado, page, pageSize, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CreditoDto>> Obtener(int id, CancellationToken ct) =>
        Ok(await service.ObtenerAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<CreditoDto>> Crear(CreditoGuardarDto dto, CancellationToken ct)
    {
        var creado = await service.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CreditoDto>> Actualizar(int id, CreditoGuardarDto dto, CancellationToken ct) =>
        Ok(await service.ActualizarAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await service.EliminarAsync(id, ct);
        return NoContent();
    }
}
