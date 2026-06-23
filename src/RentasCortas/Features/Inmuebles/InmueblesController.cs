using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RentasCortas.Features.Inmuebles;

[ApiController]
[Route("api/inmuebles")]
public class InmueblesController : ControllerBase
{
    private readonly IInmueblesService _inmueblesService;

    public InmueblesController(IInmueblesService inmueblesService)
    {
        _inmueblesService = inmueblesService;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] InmuebleFilterDTO filter)
    {
        var result = await _inmueblesService.ListAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _inmueblesService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> Create([FromBody] CreateInmuebleDTO dto)
    {
        var ownerId = GetUserId();
        var result = await _inmueblesService.CreateAsync(ownerId, dto);
        return Created(string.Empty, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInmuebleDTO dto)
    {
        var ownerId = GetUserId();
        var result = await _inmueblesService.UpdateAsync(ownerId, id, dto);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ownerId = GetUserId();
        await _inmueblesService.SoftDeleteAsync(ownerId, id);
        return Ok(new { message = "Inmueble eliminado" });
    }

    [HttpPost("{id}/images")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> AddImage(Guid id, IFormFile file)
    {
        var ownerId = GetUserId();
        var result = await _inmueblesService.AddImageAsync(ownerId, id, file);
        return Created(string.Empty, result);
    }

    [HttpDelete("{id}/images/{imageId}")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> RemoveImage(Guid id, Guid imageId)
    {
        var ownerId = GetUserId();
        await _inmueblesService.RemoveImageAsync(ownerId, id, imageId);
        return Ok(new { message = "Imagen eliminada" });
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Token inválido");
        return Guid.Parse(claim.Value);
    }
}
