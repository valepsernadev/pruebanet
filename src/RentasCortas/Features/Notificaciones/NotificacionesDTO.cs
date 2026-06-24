namespace RentasCortas.Features.Notificaciones;

public class NotificacionResponseDTO
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}