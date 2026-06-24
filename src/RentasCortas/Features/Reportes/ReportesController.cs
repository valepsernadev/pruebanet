using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RentasCortas.Features.Reportes;

[ApiController]
[Route("api/reportes")]
[Authorize(Roles = "owner")]
public class ReportesController : ControllerBase
{
    private readonly IReportesService _reportesService;

    public ReportesController(IReportesService reportesService)
    {
        _reportesService = reportesService;
    }

    [HttpGet("excel")]
    public async Task<IActionResult> DownloadExcel([FromQuery] Guid? inmuebleId, [FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate)
    {
        var ownerId = GetUserId();
        var result = await _reportesService.GenerateReportAsync(ownerId, inmuebleId, startDate, endDate);

        return File(
            result.FileContent,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            result.FileName);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("No tienes permisos para realizar esta acción");
        return Guid.Parse(claim.Value);
    }
}
