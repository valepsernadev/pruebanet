using Microsoft.AspNetCore.Http;

namespace RentasCortas.Features.Inmuebles;

public interface IInmueblesService
{
    Task<InmuebleResponseDTO> CreateAsync(Guid ownerId, CreateInmuebleDTO dto);
    Task<InmuebleResponseDTO> UpdateAsync(Guid ownerId, Guid inmuebleId, UpdateInmuebleDTO dto);
    Task SoftDeleteAsync(Guid ownerId, Guid inmuebleId);
    Task<InmuebleResponseDTO> GetByIdAsync(Guid inmuebleId);
    Task<List<InmuebleListResponseDTO>> ListAsync(InmuebleFilterDTO? filter);
    Task<ImageResponseDTO> AddImageAsync(Guid ownerId, Guid inmuebleId, IFormFile file);
    Task RemoveImageAsync(Guid ownerId, Guid inmuebleId, Guid imageId);
}