namespace RentasCortas.Models;

public class Favorito
{
    public Guid Id { get; set; }
    public Guid GuestId { get; set; }
    public Guid InmuebleId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Usuario Guest { get; set; } = null!;
    public Inmueble Inmueble { get; set; } = null!;
}