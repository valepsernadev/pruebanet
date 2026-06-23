using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentasCortas.Common.Responses;

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
        return Ok(new ApiResponse<List<InmuebleListResponseDTO>>("Inmuebles obtenidos exitosamente", 200, result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _inmueblesService.GetByIdAsync(id);
        return Ok(new ApiResponse<InmuebleResponseDTO>("Inmueble obtenido exitosamente", 200, result));
    }

    [HttpPost]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> Create([FromBody] CreateInmuebleDTO dto)
    {
        var ownerId = GetUserId();
        var result = await _inmueblesService.CreateAsync(ownerId, dto);
        return Created(string.Empty, new ApiResponse<InmuebleResponseDTO>("Inmueble creado exitosamente", 201, result));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInmuebleDTO dto)
    {
        var ownerId = GetUserId();
        var result = await _inmueblesService.UpdateAsync(ownerId, id, dto);
        return Ok(new ApiResponse<InmuebleResponseDTO>("Inmueble actualizado exitosamente", 200, result));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ownerId = GetUserId();
        await _inmueblesService.SoftDeleteAsync(ownerId, id);
        return Ok(new ApiResponse("Inmueble eliminado exitosamente", 200));
    }

    [HttpPost("{id}/images")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> AddImage(Guid id, IFormFile file)
    {
        var ownerId = GetUserId();
        var result = await _inmueblesService.AddImageAsync(ownerId, id, file);
        return Created(string.Empty, new ApiResponse<ImageResponseDTO>("Imagen subida exitosamente", 201, result));
    }

    [HttpDelete("{id}/images/{imageId}")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> RemoveImage(Guid id, Guid imageId)
    {
        var ownerId = GetUserId();
        await _inmueblesService.RemoveImageAsync(ownerId, id, imageId);
        return Ok(new ApiResponse("Imagen eliminada exitosamente", 200));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("No tienes permisos para realizar esta acción");
        return Guid.Parse(claim.Value);
    }
}