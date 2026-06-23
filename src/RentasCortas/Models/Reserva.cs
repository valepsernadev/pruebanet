namespace RentasCortas.Models;

public class Reserva
{
    public Guid Id { get; set; }
    public Guid InmuebleId { get; set; }
    public Guid GuestId { get; set; }
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public TimeOnly CheckInTime { get; set; }
    public TimeOnly CheckOutTime { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }

    public Inmueble Inmueble { get; set; } = null!;
    public Usuario Guest { get; set; } = null!;
}