using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentasCortas.Common.Responses;

namespace RentasCortas.Features.KYC;

[ApiController]
[Route("api/kyc")]
[Authorize(Roles = "guest")]
public class KYCController : ControllerBase
{
    private readonly IKYCService _kycService;

    public KYCController(IKYCService kycService)
    {
        _kycService = kycService;
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate(IFormFile image)
    {
        var userId = GetUserId();
        var result = await _kycService.ValidateAsync(userId, image);
        return Ok(new ApiResponse<KycResponseDTO>("Validación KYC procesada exitosamente", 200, result));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("No tienes permisos para realizar esta acción");
        return Guid.Parse(claim.Value);
    }
}