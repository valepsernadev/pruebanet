namespace RentasCortas.Features.Reservas;

public interface IReservasService
{
    Task<ReservaResponseDTO> CreateAsync(Guid guestId, CreateReservaDTO dto);
    Task<ReservaResponseDTO> ConfirmAsync(Guid ownerId, Guid reservaId);
    Task<ReservaResponseDTO> CancelAsync(Guid userId, Guid reservaId);
    Task<List<ReservaListResponseDTO>> ListByUserAsync(Guid userId, string role);
    Task<ReservaResponseDTO> GetByIdAsync(Guid userId, Guid reservaId);
}
