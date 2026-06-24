using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentasCortas.Common.Responses;

namespace RentasCortas.Features.Dashboard;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "owner")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMetrics([FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate)
    {
        var ownerId = GetUserId();
        var result = await _dashboardService.GetMetricsAsync(ownerId, startDate, endDate);
        return Ok(new ApiResponse<DashboardResponseDTO>("Métricas obtenidas exitosamente", 200, result));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("No tienes permisos para realizar esta acción");
        return Guid.Parse(claim.Value);
    }
}
