namespace RentasCortas.Models;

public class Review
{
    public Guid Id { get; set; }
    public Guid InmuebleId { get; set; }
    public Guid GuestId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    public Inmueble Inmueble { get; set; } = null!;
    public Usuario Guest { get; set; } = null!;
}