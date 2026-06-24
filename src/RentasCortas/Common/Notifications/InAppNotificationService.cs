using RentasCortas.Data;
using RentasCortas.Models;

namespace RentasCortas.Common.Notifications;

public class InAppNotificationService : INotificationService
{
    private readonly AppDbContext _context;

    public InAppNotificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task SendInAppAsync(Guid userId, string type, string message)
    {
        var notificacion = new Notificacion
        {
            UserId = userId,
            Type = type,
            Message = message,
            Channel = "in_app"
        };

        _context.Notificaciones.Add(notificacion);
        await _context.SaveChangesAsync();
    }
}