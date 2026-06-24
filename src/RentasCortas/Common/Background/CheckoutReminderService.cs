using Microsoft.EntityFrameworkCore;
using RentasCortas.Common.Notifications;
using RentasCortas.Data;

namespace RentasCortas.Common.Background;

public class CheckoutReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CheckoutReminderService> _logger;

    public CheckoutReminderService(IServiceScopeFactory scopeFactory, ILogger<CheckoutReminderService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndNotifyAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el ciclo de recordatorios de checkout");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task CheckAndNotifyAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var currentTime = TimeOnly.FromDateTime(now);

        var reservas = await context.Reservas
            .Include(r => r.Inmueble)
            .Include(r => r.Guest)
            .Where(r => r.Status == "confirmed" && r.CheckOut == today)
            .ToListAsync(stoppingToken);

        foreach (var reserva in reservas)
        {
            try
            {
                var reminderTime = reserva.CheckOutTime.AddHours(-1);

                if (currentTime < reminderTime || currentTime >= reserva.CheckOutTime)
                    continue;

                var reminderType = $"checkout_reminder_{reserva.Id}";

                var alreadySent = await context.Notificaciones
                    .AnyAsync(n => n.UserId == reserva.GuestId && n.Type == reminderType, stoppingToken);

                if (alreadySent)
                    continue;

                var message = $"Recordatorio: tu check-out en \"{reserva.Inmueble.Title}\" es hoy a las {reserva.CheckOutTime}. ¡Prepárate!";

                await notificationService.SendInAppAsync(reserva.GuestId, reminderType, message);
                await notificationService.SendEmailAsync(reserva.GuestId, reminderType, message);

                _logger.LogInformation("Recordatorio de checkout enviado para reserva {ReservaId}", reserva.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar recordatorio para reserva {ReservaId}", reserva.Id);
            }
        }
    }
}