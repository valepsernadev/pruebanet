namespace RentasCortas.Models;

public class PropertyImage
{
    public Guid Id { get; set; }
    public Guid InmuebleId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Inmueble Inmueble { get; set; } = null!;
}