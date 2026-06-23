using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        var guestId = GetUserId();
        var result = await _reservasService.CreateAsync(guestId, dto);
        return Created(string.Empty, result);
    }

    [HttpPatch("{id}/confirm")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var ownerId = GetUserId();
        var result = await _reservasService.ConfirmAsync(ownerId, id);
        return Ok(result);
    }

    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = GetUserId();
        var result = await _reservasService.CancelAsync(userId, id);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var userId = GetUserId();
        var role = GetUserRole();
        var result = await _reservasService.ListByUserAsync(userId, role);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        var result = await _reservasService.GetByIdAsync(userId, id);
        return Ok(result);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Token inválido");
        return Guid.Parse(claim.Value);
    }

    private string GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }
}
