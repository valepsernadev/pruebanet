namespace RentasCortas.Models;

public class ReservationStatusHistory
{
    public Guid Id { get; set; }
    public Guid ReservaId { get; set; }
    public string? PreviousStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }

    public Reserva Reserva { get; set; } = null!;
}