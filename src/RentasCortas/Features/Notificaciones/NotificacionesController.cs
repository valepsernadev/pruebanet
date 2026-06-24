using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentasCortas.Common.Responses;

namespace RentasCortas.Features.Notificaciones;

[ApiController]
[Route("api/notificaciones")]
[Authorize]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionesService _notificacionesService;

    public NotificacionesController(INotificacionesService notificacionesService)
    {
        _notificacionesService = notificacionesService;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? channel)
    {
        var userId = GetUserId();
        var result = await _notificacionesService.ListByUserAsync(userId, channel);
        return Ok(new ApiResponse<List<NotificacionResponseDTO>>("Notificaciones obtenidas exitosamente", 200, result));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("No tienes permisos para realizar esta acción");
        return Guid.Parse(claim.Value);
    }
}
