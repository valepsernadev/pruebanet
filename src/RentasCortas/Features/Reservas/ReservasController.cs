using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentasCortas.Common.Responses;

namespace RentasCortas.Features.Reservas;

[ApiController]
[Route("api/reservas")]
[Authorize]
public class ReservasController : ControllerBase
{
    private readonly IReservasService _reservasService;

    public ReservasController(IReservasService reservasService)
    {
        _reservasService = reservasService;
    }

    [HttpPost]
    [Authorize(Roles = "guest")]
    public async Task<IActionResult> Create([FromBody] CreateReservaDTO dto)
    {
        var role = GetUserRole();
        if (role != "guest")
            return StatusCode(403, new ApiResponse("Solo los huéspedes pueden crear reservas. Inicia sesión con una cuenta de tipo guest.", 403));

        var guestId = GetUserId();
        var result = await _reservasService.CreateAsync(guestId, dto);
        return Created(string.Empty, new ApiResponse<ReservaResponseDTO>("Reserva creada exitosamente", 201, result));
    }

    [HttpPatch("{id}/confirm")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var ownerId = GetUserId();
        var result = await _reservasService.ConfirmAsync(ownerId, id);
        return Ok(new ApiResponse<ReservaResponseDTO>("Reserva confirmada. Check-in: 14:00, Check-out: 12:00", 200, result));
    }

    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = GetUserId();
        var result = await _reservasService.CancelAsync(userId, id);
        return Ok(new ApiResponse<ReservaResponseDTO>("Reserva cancelada exitosamente", 200, result));
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var userId = GetUserId();
        var role = GetUserRole();
        var result = await _reservasService.ListByUserAsync(userId, role);
        return Ok(new ApiResponse<List<ReservaListResponseDTO>>("Reservas obtenidas exitosamente", 200, result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        var result = await _reservasService.GetByIdAsync(userId, id);
        return Ok(new ApiResponse<ReservaResponseDTO>("Reserva obtenida exitosamente", 200, result));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("No tienes permisos para realizar esta acción");
        return Guid.Parse(claim.Value);
    }

    private string GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }
}