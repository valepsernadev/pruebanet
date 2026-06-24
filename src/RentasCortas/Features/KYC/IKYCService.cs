namespace RentasCortas.Features.KYC;

public interface IKYCService
{
    Task<KycResponseDTO> ValidateAsync(Guid userId, IFormFile image);
}