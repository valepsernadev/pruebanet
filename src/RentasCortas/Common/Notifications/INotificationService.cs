namespace RentasCortas.Common.Notifications;

public interface INotificationService
{
    Task SendInAppAsync(Guid userId, string type, string message);
}