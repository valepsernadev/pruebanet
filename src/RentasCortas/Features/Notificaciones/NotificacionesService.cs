using Microsoft.EntityFrameworkCore;
using RentasCortas.Data;

namespace RentasCortas.Features.Notificaciones;

public class NotificacionesService : INotificacionesService
{
    private readonly AppDbContext _context;

    public NotificacionesService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<NotificacionResponseDTO>> ListByUserAsync(Guid userId, string? channel)
    {
        try
        {
            var query = _context.Notificaciones
                .Where(n => n.UserId == userId);

            if (!string.IsNullOrEmpty(channel))
                query = query.Where(n => n.Channel == channel);

            var notificaciones = await query
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();

            return notificaciones.Select(n => new NotificacionResponseDTO
            {
                Id = n.Id,
                Type = n.Type,
                Message = n.Message,
                Channel = n.Channel,
                SentAt = n.SentAt
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error al obtener las notificaciones", ex);
        }
    }
}
