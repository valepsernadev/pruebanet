namespace RentasCortas.Features.Favoritos;

public interface IFavoritosService
{
    Task<FavoritoResponseDTO> AddAsync(Guid guestId, Guid inmuebleId);
    Task RemoveAsync(Guid guestId, Guid inmuebleId);
    Task<List<FavoritoResponseDTO>> ListByGuestAsync(Guid guestId);
}