namespace RentasCortas.Models;

public class Notificacion
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Usuario User { get; set; } = null!;
}