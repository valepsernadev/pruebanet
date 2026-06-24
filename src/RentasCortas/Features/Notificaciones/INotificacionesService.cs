namespace RentasCortas.Features.Notificaciones;

public interface INotificacionesService
{
    Task<List<NotificacionResponseDTO>> ListByUserAsync(Guid userId, string? channel);
}
