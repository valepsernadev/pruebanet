using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentasCortas.Common.Responses;

namespace RentasCortas.Features.Favoritos;

[ApiController]
[Route("api/favoritos")]
[Authorize(Roles = "guest")]
public class FavoritosController : ControllerBase
{
    private readonly IFavoritosService _favoritosService;

    public FavoritosController(IFavoritosService favoritosService)
    {
        _favoritosService = favoritosService;
    }

    [HttpPost("{inmuebleId}")]
    public async Task<IActionResult> Add(Guid inmuebleId)
    {
        var guestId = GetUserId();
        var result = await _favoritosService.AddAsync(guestId, inmuebleId);
        return Created(string.Empty, new ApiResponse<FavoritoResponseDTO>("Inmueble agregado a favoritos", 201, result));
    }

    [HttpDelete("{inmuebleId}")]
    public async Task<IActionResult> Remove(Guid inmuebleId)
    {
        var guestId = GetUserId();
        await _favoritosService.RemoveAsync(guestId, inmuebleId);
        return Ok(new ApiResponse("Inmueble eliminado de favoritos", 200));
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var guestId = GetUserId();
        var result = await _favoritosService.ListByGuestAsync(guestId);
        return Ok(new ApiResponse<List<FavoritoResponseDTO>>("Favoritos obtenidos exitosamente", 200, result));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("No tienes permisos para realizar esta acción");
        return Guid.Parse(claim.Value);
    }
}