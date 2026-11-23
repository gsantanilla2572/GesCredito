using GesCredito.Services.Dtos;
using GesCredito.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GesCredito.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController(IClienteService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ClienteDto>>> Listar(
        [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default) =>
        Ok(await service.ListarAsync(search, page, pageSize, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDetalleDto>> Obtener(int id, CancellationToken ct) =>
        Ok(await service.ObtenerAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Crear(ClienteGuardarDto dto, CancellationToken ct)
    {
        var creado = await service.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClienteDto>> Actualizar(int id, ClienteGuardarDto dto, CancellationToken ct) =>
        Ok(await service.ActualizarAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await service.EliminarAsync(id, ct);
        return NoContent();
    }
}
