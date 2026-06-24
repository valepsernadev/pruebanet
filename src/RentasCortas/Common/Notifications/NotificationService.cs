using Microsoft.EntityFrameworkCore;
using RentasCortas.Data;
using RentasCortas.Models;

namespace RentasCortas.Common.Notifications;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly EmailNotificationService _emailService;

    public NotificationService(AppDbContext context, EmailNotificationService emailService)
    {
        _context = context;
        _emailService = emailService;
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

    public async Task SendEmailAsync(Guid userId, string type, string message)
    {
        var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return;

        await _emailService.SendAsync(user.Email, type, message);

        var notificacion = new Notificacion
        {
            UserId = userId,
            Type = type,
            Message = message,
            Channel = "email"
        };

        _context.Notificaciones.Add(notificacion);
        await _context.SaveChangesAsync();
    }
}
