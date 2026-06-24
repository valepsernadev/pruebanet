using Microsoft.EntityFrameworkCore;
using RentasCortas.Data;
using RentasCortas.Models;

namespace RentasCortas.Common.Notifications;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly EmailNotificationService _emailService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(AppDbContext context, EmailNotificationService emailService, ILogger<NotificationService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
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
        if (user == null)
        {
            _logger.LogWarning("No se encontró el usuario {UserId} para enviar email de tipo {Type}", userId, type);
            return;
        }

        _logger.LogDebug("Delegando envío de email a {Email} para usuario {UserId}, tipo: {Type}", user.Email, userId, type);
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
